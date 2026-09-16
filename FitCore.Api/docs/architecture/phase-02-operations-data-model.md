# Phase 02 — Operations data model

## Catalog and entitlement

- **Service** — tenant catalog entry (what the gym offers). Deactivating a service also deactivates all of its active plans. Existing memberships are unchanged.
- **MembershipPlan** — priced offer on a service (`SessionPack` or `TimePeriod`). Unique name per tenant. May be inactive while the service stays active; cannot stay active under an inactive service.
- **Membership** — member’s entitlement from a plan (status, dates, sessions remaining). Admin cancel (`Active`/`Frozen` → `Cancelled`) stores `CancelReason` (`MemberRequest` | `AdminDecision`), optional note, `CancelledAt`, `CancelledByStaffId` (JWT actor). When no `Active` memberships remain, member becomes `Paused`.



