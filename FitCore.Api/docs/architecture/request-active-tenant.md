# Request active tenant gate

Opt-in filter for organization Admin APIs that already carry a JWT `tenant_id` claim.

## Attribute

`[RequireActiveTenant]` on a controller or action:

1. Parse `tenant_id` (else `missing-tenant-context`).
2. Load tenant via `ITenantStore` (else `organization-not-found`).
3. Require `TenantStatus.Active` (else `organization-not-active`).
4. Set scoped `ITenantContext.TenantId`.

## Rules

- Use on TenantOwner Admin controllers that need an active org.
- Do **not** put on Platform, register/login, invite accept, or other routes without that claim / that must run when the org is inactive.
- Services may keep an explicit `Guid tenantId` argument (pass `tenantContext.TenantId` from the controller). Do not re-check exists/active inside those gated use cases.

## Rollout

Done for TenantOwner Admin: Members, Staff, Services, Plans, Memberships, Visits.

Still out of scope: Platform, `OrganizationsController` register/login, member/staff auth portals.
