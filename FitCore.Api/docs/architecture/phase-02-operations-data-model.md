# Phase 02 — Operations data model

## Catalog and entitlement

- **Service** — tenant catalog entry (what the gym offers).
- **MembershipPlan** — priced offer on a service (`SessionPack` or `TimePeriod`). Unique name per tenant.
- **Membership** — member’s entitlement from a plan (status, dates, sessions remaining). Admin cancel (`Active`/`Frozen` → `Cancelled`) stores `CancelReason` (`MemberRequest` | `AdminDecision`), optional note, `CancelledAt`, `CancelledByStaffId` (JWT actor). When no `Active` memberships remain, member becomes `Paused`.



