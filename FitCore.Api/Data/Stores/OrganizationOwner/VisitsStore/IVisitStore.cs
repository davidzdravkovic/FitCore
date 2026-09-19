using FitCore.Api.Domain.Memberships;
using FitCore.Api.Domain.Staffs;
using FitCore.Api.Domain.Visits;

namespace FitCore.Api.Data.Stores.OrganizationOwner.VisitsStore;

public interface IVisitStore
{
    Task<VisitLockKeys?> FindLockKeysForVisitAsync(
        Guid tenantId,
        Guid visitId,
        CancellationToken cancellationToken = default);

    Task<Visit?> FindByIdForVoidAsync(
        Guid tenantId,
        Guid visitId,
        CancellationToken cancellationToken = default);

    Task<Membership?> FindMembershipForScheduleAsync(
        Guid tenantId,
        Guid membershipId,
        CancellationToken cancellationToken = default);

    Task<int> CountActiveMembershipsForMemberAsync(
        Guid tenantId,
        Guid memberId,
        Guid excludeMembershipId,
        CancellationToken cancellationToken = default);

    Task<Staff?> FindActiveCoachAsync(
        Guid tenantId,
        Guid coachStaffId,
        CancellationToken cancellationToken = default);

    Task<bool> HasOpenCoachOverlapAsync(
        Guid tenantId,
        Guid coachStaffId,
        DateTime startAt,
        DateTime endAt,
        CancellationToken cancellationToken = default,
        Guid? excludeVisitId = null);

    Task<bool> HasOpenMemberOverlapAsync(
        Guid tenantId,
        Guid memberId,
        DateTime startAt,
        DateTime endAt,
        CancellationToken cancellationToken = default,
        Guid? excludeVisitId = null);

    Task<bool> HasOccupyingCoachOverlapAsync(
        Guid tenantId,
        Guid coachStaffId,
        DateTime startAt,
        DateTime endAt,
        CancellationToken cancellationToken = default,
        Guid? excludeVisitId = null);

    Task<bool> HasOccupyingMemberOverlapAsync(
        Guid tenantId,
        Guid memberId,
        DateTime startAt,
        DateTime endAt,
        CancellationToken cancellationToken = default,
        Guid? excludeVisitId = null);

    Task<IReadOnlyList<Visit>> ListByTenantAsync(
        Guid tenantId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    Task AddAsync(Visit visit);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed record VisitLockKeys(Guid MembershipId, Guid MemberId);
