# Domain current rules for data states

## Plan

Plan has states: `active` and `inactive`.
A plan depends on an `active service`.

Inserting a plan checks that invariant which can be broken under concurrency, because in the same time service invalidation (`active -> inactive`) can happen concurrently even though the inactivation of the serivce inactivates all the plans depended on that service. But the plan that reads service `active` and is about from that point to be created will not be catched by service invalidation that checks active plans - for this plan does not exists yet. Then the invariant can be broken and we can end up with `active plan - inactive service`.

### Fix

Set a `select for update lock on the service` both service invalidation and plan creation will race for this lock.
Case `service invalidation wins`: plan creation fails due the state service check.
Case `plan creation wins`: plan creation succed, but the service catches active plan -> turns into inactive.

### Tradeoff

Currently we can affort this incosistence to happen, one admin UI or later maybe fews can end up in this extreme rare window.
No need for extra depth optimizations.


## Membership

Membership has states: `active`, `frozen`, `expired`, `cancelled`.
Membership depends on: `active` plan and  `assignable` -> (`active`, `lead` and `paused`) member.

Assigning a membership checks on member for those "valid" states even they are many we have two types of states `assignable` and none `assignable`, and `active` plan. So under concurrency we have for membership 2 moving independent paths that can make inconsistency. For two possible states on two independed moving parts we have 4 permutations on which just one permutation is valid for our domain invariant -> `active plan and active member`.

### Wrong member states

| Member | Plan | Membership|
|---------|--------|----------------|
| `paused` | `active` | `paused member` `active plan` and `active membership`|
| `cancelled` | `active` | `cancelled member` `active plan` and `active membership`|

The `paused` and `cancellation` paths are different:

1. `Paused` state transition happens after there is no `active membership`. Visits operations that are draining sessions -> `check in` or just on `scheduled` will check for active memberships if this is the last session drained and therefore as consequence if there is no active membership for that member will turn the member into `paused` state.
The inconsistency for this can happen if this `paused` actions read no active memberships, and after the saved membership with active member in the DB then the update for paused overwrites the active member. We can end up with row one from the table.

2. `Cancellation` state transition happens on explicit cancellation by the admin. And mirrors the same race mechanism as `paused`.

### Fix

Set a lock on the `select for update lock on the member`.
Case: `Visit operation wins` sets the member `paused`, the membership assign waits and then updates the member into active with active membership.
Case: `Membership wins` sets the client with active membership into `active`, the visit returns due the constraint there is a valid memberships.

### Tradeoffs 

`Cancellation` since is a explicit admin action just the admin or few admins, is rare that an admin in the same time will cancel and assign a membership so weird window + weird action from an admin. 
`Paused` paths which can be trigger by coaches in the same time N coaches check in/schedule concurrently if they (forgot), and the admin assigns a membership so this case needs a considiration on implementing the lock for consistency.

### Wrong plan states 

Modifying the plan into `inactive` while the membership reads `active` plan is possible to happen but is a rare since is admin action and can rarely happen and in concurrency narrow window is also rare and weird. And now that is allowed so for inactivation on plans are not in any case affecting the old memberships, the rule is do not assign a new membership on invalid plan, those that are assigned needs to be resolved and they can exists with legacy plan or service.


## Visit

Visit has states: `scheduled`, `postponed`, `expired`, `void`, `cancelled`.
Valid visit needs a:`active` member, `active` coach and `active` membership.

The implemented membership lock is protecting changes on the membership states and client while creating the visit. The visit creates if `active` member and membership is, the member is protected behind the membership since the member state depends on the membership state  for changing to -> cancelled or paused. The membership protects the session bucked not being disposed concurrently.

Member lock, protects a member to schedule overlap slot on different memberships, if two concurrent visits are created with same membership, membership lock protects it but two different memberships same member the member lock does not allow overlap time scheduled.

Coach lock, same as for member does not allow coach to be scheduled with overlap time if there are concurrent visits called.

## Rule 

All cases where there are many modifiers concurrently many coaches with admins, those use cases needs to be understanded and eventually a `lock` to be implement
Cases where there are only admin actions, admin will `rarely` contradict himself even if that happens the time window is so narrow which makes the problem almost impossible to occur and those cases are not payment or other inconsistencies for to be consider implementation of `locks`.
















