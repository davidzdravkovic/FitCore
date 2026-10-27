# Phase 02 — Operations data model

Describes in plain words what business rules are applied with the phase two data model.

## Service

What the gym offers in its catalog. Belongs to one tenant.

One unique name per tenant: the same gym cannot list two services with the same name.

Can be active or inactive. Turning a service off is an application rule that also turns off its active plans; the table itself only stores whether this service is active.

## Membership plan

A priced session pack offered on one service. Belongs to one tenant and one service.

One unique name per tenant.

The database only allows session packs: entitlement must be `SessionPack`, and session count must be present and greater than zero.

Price is a money amount with fixed scale. The plan can be active or inactive on its own; an inactive plan stays in the table so existing memberships can still show the plan name. The service cannot be removed while plans still point at it.

## Membership

A member’s pack snapshot after a plan is assigned. Belongs to one tenant, one member, and one plan.

Stores three counters: total sessions frozen from the plan at assign, sessions held by open scheduled visits, and sessions already burned. Available is not a column; it is total minus reserved minus burned. The database refuses negative counters and refuses a total smaller than reserved plus burned.

Status is stored as text (`Active`, `Frozen`, `Expired`, `Cancelled`). Cancel can keep a reason, a short note, a time, and which staff cancelled. The plan row cannot be deleted while memberships still reference it.

There is an index on tenant, member, and status so looking up a member’s memberships by status stays cheap.

## Visit

A training slot on one membership. Belongs to one tenant, one membership, one member, one coach, and one service.

End time must be after start time.

Status is stored as text (`Scheduled`, `Completed`, `NoShow`, `Postponed`, `Cancelled`, `Voided`). Void can keep a short note and which staff voided. Membership, member, coach, and service rows cannot be deleted while visits still reference them.

While status is `Scheduled`, the same tenant cannot store two visits that are the exact same membership, coach, start, and end; that duplicate is blocked with a partial unique index. Other statuses may repeat those times after the visit is closed.

Indexes on tenant with start, with coach and start, with member and start, and with membership and status support calendar and pack lookups.
