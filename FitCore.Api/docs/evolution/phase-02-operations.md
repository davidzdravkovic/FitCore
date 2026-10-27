# Phase 02 — Operations data model

## Catalog and entitlement

- **Service** — tenant catalog entry (what the gym offers). Deactivating a service also deactivates all of its active plans. Existing memberships are unchanged.
- **MembershipPlan** — priced session-pack offer on a service (`SessionPack` + `SessionCount`). Unique name per tenant. May be inactive while the service stays active; cannot stay active under an inactive service. Inactive plans remain readable for existing memberships (name); new assigns require an active plan.
- **Membership** — member’s entitlement snapshot: `SessionTotal` (frozen from plan at assign), `SessionsReserved` (scheduled holds), `SessionsBurned` (resolved credits). Available = Total − Reserved − Burned. Admin cancel (`Active`/`Frozen` → `Cancelled`) stores cancel audit; open visits are cancelled and reserved credits forfeited to burned. When no `Active` memberships remain, member becomes `Paused`.
- **Visit** — scheduled training slot on an `Active` membership (member, coach, service, start/end). `Scheduled` reserves pack credit (`ConsumedSessionCredit`); `Completed` / `NoShow` / late `Cancelled` keep credit burned; `Postponed` / `Voided` restore credit. Open visits occupy the calendar slot; reschedule keeps status `Scheduled` and moves time and/or coach. Membership cancel cancels open visits first.
