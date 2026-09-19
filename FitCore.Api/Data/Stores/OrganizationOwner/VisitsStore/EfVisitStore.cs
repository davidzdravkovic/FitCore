using FitCore.Api.Domain.Memberships;
using FitCore.Api.Domain.Staffs;
using FitCore.Api.Domain.Visits;
using Microsoft.EntityFrameworkCore;

namespace FitCore.Api.Data.Stores.OrganizationOwner.VisitsStore;

public class EfVisitStore(AppDbContext db) : IVisitStore
{
    public async Task<VisitLockKeys?> FindLockKeysForVisitAsync(
        Guid tenantId,
        Guid visitId,
        CancellationToken cancellationToken = default)
    {
        return await db.Visits
            .AsNoTracking()
            .Where(v => v.TenantId == tenantId && v.Id == visitId)
            .Select(v => new VisitLockKeys(v.MembershipId, v.MemberId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Visit?> FindByIdForVoidAsync(
        Guid tenantId,
        Guid visitId,
        CancellationToken cancellationToken = default)
    {
        return await db.Visits
            .Include(v => v.Membership)
            .Include(v => v.Member)
            .Include(v => v.CoachStaff)
            .Include(v => v.Service)
            .FirstOrDefaultAsync(
                v => v.TenantId == tenantId && v.Id == visitId,
                cancellationToken);
    }

    public Task<Membership?> FindMembershipForScheduleAsync(
        Guid tenantId,
        Guid membershipId,
        CancellationToken cancellationToken = default)
    {
        return db.Memberships
            .Include(m => m.Member)
            .Include(m => m.Plan)
                .ThenInclude(p => p.Service)
            .FirstOrDefaultAsync(
                m => m.TenantId == tenantId && m.Id == membershipId,
                cancellationToken);
    }

    public Task<int> CountActiveMembershipsForMemberAsync(
        Guid tenantId,
        Guid memberId,
        Guid excludeMembershipId,
        CancellationToken cancellationToken = default)
    {
        return db.Memberships.CountAsync(
            m => m.TenantId == tenantId
                && m.MemberId == memberId
                && m.Id != excludeMembershipId
                && m.Status == MembershipStatus.Active,
            cancellationToken);
    }

    public Task<Staff?> FindActiveCoachAsync(
        Guid tenantId,
        Guid coachStaffId,
        CancellationToken cancellationToken = default)
    {
        return db.Staff
            .FirstOrDefaultAsync(
                s => s.TenantId == tenantId
                    && s.Id == coachStaffId
                    && s.DeletedAt == null,
                cancellationToken);
    }

    public Task<bool> HasOpenCoachOverlapAsync(
        Guid tenantId,
        Guid coachStaffId,
        DateTime startAt,
        DateTime endAt,
        CancellationToken cancellationToken = default,
        Guid? excludeVisitId = null)
    {
        return HasCoachOverlapAsync(
            tenantId,
            coachStaffId,
            startAt,
            endAt,
            VisitStatusRules.Open,
            cancellationToken,
            excludeVisitId);
    }

    public Task<bool> HasOpenMemberOverlapAsync(
        Guid tenantId,
        Guid memberId,
        DateTime startAt,
        DateTime endAt,
        CancellationToken cancellationToken = default,
        Guid? excludeVisitId = null)
    {
        return HasMemberOverlapAsync(
            tenantId,
            memberId,
            startAt,
            endAt,
            VisitStatusRules.Open,
            cancellationToken,
            excludeVisitId);
    }

    public Task<bool> HasOccupyingCoachOverlapAsync(
        Guid tenantId,
        Guid coachStaffId,
        DateTime startAt,
        DateTime endAt,
        CancellationToken cancellationToken = default,
        Guid? excludeVisitId = null)
    {
        return HasCoachOverlapAsync(
            tenantId,
            coachStaffId,
            startAt,
            endAt,
            VisitStatusRules.OccupiesSlot,
            cancellationToken,
            excludeVisitId);
    }

    public Task<bool> HasOccupyingMemberOverlapAsync(
        Guid tenantId,
        Guid memberId,
        DateTime startAt,
        DateTime endAt,
        CancellationToken cancellationToken = default,
        Guid? excludeVisitId = null)
    {
        return HasMemberOverlapAsync(
            tenantId,
            memberId,
            startAt,
            endAt,
            VisitStatusRules.OccupiesSlot,
            cancellationToken,
            excludeVisitId);
    }

    private Task<bool> HasCoachOverlapAsync(
        Guid tenantId,
        Guid coachStaffId,
        DateTime startAt,
        DateTime endAt,
        VisitStatus[] blocking,
        CancellationToken cancellationToken,
        Guid? excludeVisitId)
    {
        return db.Visits.AnyAsync(
            v => v.TenantId == tenantId
                && v.CoachStaffId == coachStaffId
                && blocking.Contains(v.Status)
                && (excludeVisitId == null || v.Id != excludeVisitId)
                && v.StartAt < endAt
                && v.EndAt > startAt,
            cancellationToken);
    }

    private Task<bool> HasMemberOverlapAsync(
        Guid tenantId,
        Guid memberId,
        DateTime startAt,
        DateTime endAt,
        VisitStatus[] blocking,
        CancellationToken cancellationToken,
        Guid? excludeVisitId)
    {
        return db.Visits.AnyAsync(
            v => v.TenantId == tenantId
                && v.MemberId == memberId
                && blocking.Contains(v.Status)
                && (excludeVisitId == null || v.Id != excludeVisitId)
                && v.StartAt < endAt
                && v.EndAt > startAt,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Visit>> ListByTenantAsync(
        Guid tenantId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        return await db.Visits
            .AsNoTracking()
            .Include(v => v.Member)
            .Include(v => v.CoachStaff)
            .Include(v => v.Service)
            .Where(v =>
                v.TenantId == tenantId
                && v.StartAt < to
                && v.EndAt > from)
            .OrderBy(v => v.StartAt)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Visit visit)
    {
        db.Visits.Add(visit);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
