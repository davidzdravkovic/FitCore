using FitCore.Api.Domain.Members;
using FitCore.Api.Domain.Memberships;
using FitCore.Api.Domain.Plans;
using FitCore.Api.Domain.Visits;

namespace FitCore.Api.Data.Stores.OrganizationOwner.MembershipsStore;

public interface IMembershipStore
{
    Task<IReadOnlyList<Membership>> ListByTenantAsync(
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

    Task<Membership?> FindByIdForCancelAsync(
        Guid tenantId,
        Guid membershipId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Visit>> ListOpenVisitsForMembershipAsync(
        Guid tenantId,
        Guid membershipId,
        CancellationToken cancellationToken = default);

    Task<int> CountActiveMembershipsForMemberAsync(
        Guid tenantId,
        Guid memberId,
        Guid excludeMembershipId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Membership membership);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
