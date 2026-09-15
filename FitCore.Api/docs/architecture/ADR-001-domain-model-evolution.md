# Domain evolution stragety

The evolution is going to be done in `four` phases:

1. Entities called `Identities` - Users (Staff, members), Tenants.
2. `Operational Domain` - entities required to implement business operations, such as Memberships, Services, Scheduling, and Payments.
3. `Business Intelligence (BI)` - derived information and read models based on the established operational domain
4. `Optimization` - modifications to the model and its supporting infrastructure to improve performance without violating established business rules and invariants.

Business rules discovered in each phase drive the evolution of the domain model.
The evolution does not expect a perfect model from beginning since the business rules can not be known upfront as a whole.

---

## Phase one - Identities

The goal of this phase is to establish the foundational entities on which the rest of the domain will be built.

An initial business requirement is the concept of `free clients`: clients that are recognized by the platform and are granted direct access to the product without an existing tenant relationship.

To satisfy this requirement, the domain must enforce the following invariant:

> `Only clients who have been invited can create one free tenant`.

This requirement introduces additional foundational entities and relationships, such as: 
`PlatformAdmin`, `PlatformLoginToken` required entities for authenticated way of platfrom admin login (creation was made from seed).
`Invitation` needed for tenant registration on dedicated/known tenant by the platform.
`StaffInvite` required for giving access for the staff to his own workspace by creating credentials same for members `MemberInvite`.

 Note: `Invitation` is not used for members and staff invite since the constraints are different, they are created by the tenant admin before they are invited this is just ability to set the credentials to their workspace.  


The resulting flow is:

```text
PlatformAdmin → Invitation Token → Tenant Registration → Tenant + Admin User
```

At this stage, the model focuses on identity and tenant ownership rather than the complete lifecycle of the business.

---

## Phase two - Operation entities

There are interactions between member <- coach, member <- admin, staff <- admin and eventually platform -> tenant with well defined rules by the tenant/business and the platform. Puting aside the platform business operations, first prerequsit for tenant operations is  operatable member, operetable member is member that has a defined state and lifecycle by the business. The state would be represented with `Membership`. The business can operate on memberships a defined interface that covers the member.

Identity phase promises isolation on tenant managing its own `resources` for creation and deletion, plus a few states on a created identities.
The membership is the glue that connects a promise from the identity: only valid states (active, paused, lead) on a client can acquire a membership and membership is representation on valid client which the operations expect as a promise, changing a clients state -> changes the membership state therefore the client capalities in the tenant.

## Responsibilities and their semantics on each Layer. 

The member states can be viewed in semantic meaning as states that are `valid` or `invalid` to obtain a memberships.
But now they are 4 which will be used for statistics.

The client can have 4 states:

1. `Lead` -> semantic meaning same as active and paused in terms of capability to obtain a membership. The state exists statistics.
2. `Active` -> has at least one membership.
3. `Paused` -> has no active membership, but used to have.
4. `Cancelled` -> soft delete, not physical but is not included into operations/statistics. 

Client possible state convertions:

From every state to `Cancelled` but from `Active` the admin should resolve first the `Active` or `Frozen` memberships if any before cancelling.
From `Lead/Paused` to `Active` and from `Active` to `Paused`, this are automatic conversions from the app, on activating the first or expiring the last membership.

So `Active` or `Paused` or `Lead` member can obtain a membership.

The membership can have 4 states:

Same as members state they are invalid or valid for obtaining a schedule but are used for the statistic later BI.

1. `Active`-> usable entitlement. Starts from assinging,
2. `Frozen` -> at hold, is just for statistics. The admin changes this state.
3. `Expired` -> the client successfuly finished the membership. Automatic on the last day/session.
4. `Cancelled` -> from Active the membership is discarded, the admin should explain why, and that will be in v1 recorded in a DB. Admin manually cancells.

Conversions:

1. `Active` to `Frozen` on hold, but used for now as a statistics, `Expired` the membership when ends via the membership date/sessions  expiration. `Cancelled` admin discards a membership which is still valid.
2. `Frozen` to `Active`on admin continuation and `Cancelled` when the admin discards a valid membership.
3. `Expired` to none.
4. `Cancelled` maybe reversible to `Active` or `Frozen` not to `Expired`. 


The states are interface that is exposed to the scheduling and scheduling is operating just on `Active` memberships and does not care about members and their state. Those rules are explictily written in Domain/Policy.


### 1. Make operative member

`Membership` is composition of 2 ideas, what the business offers and what the client chooses. `Membership` is a model combination of the existing `Plan`. The `Plan` is composed of existing `Service` which carries the different catalog of services and a payload that gives a value to specific service with `price and sessions or duration of days`. The member gives identity to the membership with time and number of disposed sessions.

### 1.1 Member acquiring membership

For FitCore v1, payment is **outbound** (outside the app). Admin assigns a membership from a catalog plan.

