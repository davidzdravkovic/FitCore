# Row lock ordering (global)

Deadlock prevention for FitCore.Api: every use case that takes multiple row locks must follow **one total order**. Skipping locks you do not need is allowed; going **backwards** is not.

## Order

```text
1. Membership
2. Member
3. Coach
4. Visit
```

| Rank | Resource | Typical reason |
|------|----------|----------------|
| 1 | `Memberships` | Pack credit / entitlement status |
| 2 | `Members` | Member calendar overlap / Active↔Paused |
| 3 | `Staff` (coach) | Coach calendar overlap |
| 4 | `Visits` | Single-visit transitions (void, etc.) |

## Rules

1. Acquire locks only via `IOrderedRowLocks` / `IOrderedRowLockScope` (store locking helper).
2. Call lock methods in increasing rank only (e.g. Membership → Coach is OK; Coach → Membership throws).
3. A use case may take a **subset** (e.g. assign membership: Member only).
4. Do not add ad-hoc `FOR UPDATE` outside the helper.
5. Same order applies to any future writer (resolve, void, postpone).

## Current use cases

| Use case | Locks |
|----------|--------|
| Create visit | Membership → Member → Coach |
| Record past visit (Completed / NoShow) | Membership → Member → Coach |
| Assign membership | Member |
| Cancel membership | Membership → Member |
| Cancel member | Member |
| Void visit | Membership → Member → Visit (membership + member ids peeked without lock first) |
| Resolve visit (Completed / NoShow) | Membership → Member → Visit (same peek; may expire membership + pause member) |
| Reschedule visit | Membership → Member → Coach (target) → Visit (times and/or coach; status stays Scheduled) |

## Why

If all transactions acquire locks according to a fixed total order, the wait-for graph cannot cycle → classic multi-lock deadlock is prevented. Postgres may still abort on bugs that ignore this doc.
