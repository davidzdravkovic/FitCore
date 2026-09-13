using FitCore.Api.Domain.Entities;

namespace FitCore.Api.Data.Stores.OrganizationOwner.MembershipsStore;

public interface IMembershipStore
{
    Task<IReadOnlyList<Membership>> ListByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<Tenant?> FindTenantByIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<Member?> FindActiveMemberAsync(
        Guid tenantId,
        Guid memberId,
        CancellationToken cancellationToken = default);

    Task<MembershipPlan?> FindActivePlanAsync(
        Guid tenantId,
        Guid planId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Membership membership);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
