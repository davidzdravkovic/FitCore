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

Identity phase promises isolation on tenant managing its own `resources` for creation and leave-roster (`Cancelled`), plus a few states on created identities.
The membership is the glue that connects a promise from the identity: `Lead`, `Paused`, and `Active` can receive a membership assignment; the membership is what the operations care about. Assign promotes `Lead` or `Paused` to `Active`.
`Lead` is a new prospect created via Add member (always starts as Lead). `Paused` is a known client with no ongoing usable membership (Import v1: same fields as create, status Paused). `Active` means they have entitlement they can dispose — only via assign, never on create. `Cancelled` leaves the roster (contacts free, out of default lists/BI). Temporary entitlement holds use `MembershipStatus.Frozen`, not member Paused.
Cancelling a member is refused while any `Active` or `Frozen` membership remains; the admin must resolve those entitlements first (policy/liability). Only then can the member be set to `Cancelled`.
Admin cancels a membership with reason `MemberRequest` or `AdminDecision` (actor = owner staff id from JWT). After cancel, if the member has no remaining `Active` memberships, member status becomes `Paused`.

### 1. Make operative member

`Membership` is composition of 2 ideas, what the business offers and what the client chooses. `Membership` is a model combination of the existing `Plan`. The `Plan` is composed of existing `Service` which carries the different catalog of services and a payload that gives a value to specific service with `price and sessions`. The member gives identity to the membership with time and number of disposed sessions.

### 1.1 Member acquiring membership

For FitCore v1, payment is **outbound** (outside the app). Admin assigns a membership from a catalog plan.

