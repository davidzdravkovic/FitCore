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
