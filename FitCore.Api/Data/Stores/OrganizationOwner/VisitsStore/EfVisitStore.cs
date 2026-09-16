using FitCore.Api.Domain.Memberships;
using FitCore.Api.Domain.Staffs;
using FitCore.Api.Domain.Visits;
using Microsoft.EntityFrameworkCore;

namespace FitCore.Api.Data.Stores.OrganizationOwner.VisitsStore;

public class EfVisitStore(AppDbContext db) : IVisitStore
{
    public async Task<Guid?> FindMembershipIdForVisitAsync(
        Guid tenantId,
        Guid visitId,
        CancellationToken cancellationToken = default)
    {
        return await db.Visits
            .AsNoTracking()
            .Where(v => v.TenantId == tenantId && v.Id == visitId)
            .Select(v => (Guid?)v.MembershipId)
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
        CancellationToken cancellationToken = default)
    {
        return db.Visits.AnyAsync(
            v => v.TenantId == tenantId
                && v.CoachStaffId == coachStaffId
                && VisitStatusRules.Open.Contains(v.Status)
                && v.StartAt < endAt
                && v.EndAt > startAt,
            cancellationToken);
    }

    public Task<bool> HasOpenMemberOverlapAsync(
        Guid tenantId,
        Guid memberId,
        DateTime startAt,
        DateTime endAt,
        CancellationToken cancellationToken = default)
    {
        return db.Visits.AnyAsync(
            v => v.TenantId == tenantId
                && v.MemberId == memberId
                && VisitStatusRules.Open.Contains(v.Status)
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
