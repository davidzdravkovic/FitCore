using FitCore.Api.Domain.Members;
using FitCore.Api.Domain.Memberships;
using FitCore.Api.Domain.Plans;
using FitCore.Api.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace FitCore.Api.Data.Stores.OrganizationOwner.MembershipsStore;

public class EfMembershipStore(AppDbContext db) : IMembershipStore
{
    public async Task<IReadOnlyList<Membership>> ListByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.Memberships
            .AsNoTracking()
            .Include(m => m.Member)
            .Include(m => m.Plan)
            .Where(m => m.TenantId == tenantId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Tenant?> FindTenantByIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return db.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
    }

    public Task<Member?> FindActiveMemberAsync(
        Guid tenantId,
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        return db.Members
            .FirstOrDefaultAsync(
                m => m.TenantId == tenantId
                    && m.Id == memberId
                    && MemberStatusRules.CanAssign.Contains(m.Status),
                cancellationToken);
    }

    public Task<MembershipPlan?> FindActivePlanAsync(
        Guid tenantId,
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        return db.MembershipPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.TenantId == tenantId && p.Id == planId && p.IsActive,
                cancellationToken);
    }

    public Task<Membership?> FindByIdForCancelAsync(
        Guid tenantId,
        Guid membershipId,
        CancellationToken cancellationToken = default)
    {
        return db.Memberships
            .Include(m => m.Member)
            .Include(m => m.Plan)
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

    public Task AddAsync(Membership membership)
    {
        db.Memberships.Add(membership);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
