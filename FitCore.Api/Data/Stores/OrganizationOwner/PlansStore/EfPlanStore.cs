using FitCore.Api.Domain.Plans;
using FitCore.Api.Domain.Services;
using FitCore.Api.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace FitCore.Api.Data.Stores.OrganizationOwner.PlansStore;

public class EfPlanStore(AppDbContext db) : IPlanStore
{
    public async Task<IReadOnlyList<MembershipPlan>> ListByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.MembershipPlans
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.CreatedAt)
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

    public Task<Service?> FindActiveServiceAsync(
        Guid tenantId,
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        return db.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.TenantId == tenantId && s.Id == serviceId && s.IsActive,
                cancellationToken);
    }

    public Task<bool> NameTakenAsync(
        Guid tenantId,
        string name,
        CancellationToken cancellationToken = default)
    {
        return db.MembershipPlans.AnyAsync(
            p => p.TenantId == tenantId && p.Name == name,
            cancellationToken);
    }

    public Task AddAsync(MembershipPlan plan)
    {
        db.MembershipPlans.Add(plan);
        return Task.CompletedTask;
    }

    public Task<MembershipPlan?> FindByIdAsync(
        Guid tenantId,
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        return db.MembershipPlans.FirstOrDefaultAsync(
            p => p.TenantId == tenantId && p.Id == planId,
            cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
