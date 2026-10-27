# Phase 02 — Operations data model

## Catalog and entitlement

- **Service** — tenant catalog entry (what the gym offers). Deactivating a service also deactivates all of its active plans. Existing memberships are unchanged.
- **MembershipPlan** — priced session-pack offer on a service (`SessionPack` + `SessionCount`). Unique name per tenant. May be inactive while the service stays active; cannot stay active under an inactive service. Inactive plans remain readable for existing memberships (name); new assigns require an active plan.
- **Membership** — member’s entitlement snapshot: `SessionTotal` (frozen from plan at assign), `SessionsReserved` (scheduled holds), `SessionsBurned` (resolved credits). Available = Total − Reserved − Burned. Admin cancel (`Active`/`Frozen` → `Cancelled`) stores cancel audit; open visits are cancelled and reserved credits forfeited to burned. When no `Active` memberships remain, member becomes `Paused`.



