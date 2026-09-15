using FitCore.Api.Domain.Services;
using FitCore.Api.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace FitCore.Api.Data.Stores.OrganizationOwner.ServicesStore;

public class EfServiceStore(AppDbContext db) : IServiceStore
{
    public async Task<IReadOnlyList<Service>> ListByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.Services
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.CreatedAt)
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

    public Task<bool> NameTakenAsync(
        Guid tenantId,
        string name,
        CancellationToken cancellationToken = default)
    {
        return db.Services.AnyAsync(
            s => s.TenantId == tenantId && s.Name == name,
            cancellationToken);
    }

    public Task AddAsync(Service service)
    {
        db.Services.Add(service);
        return Task.CompletedTask;
    }

    public Task<Service?> FindByIdAsync(
        Guid tenantId,
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        return db.Services.FirstOrDefaultAsync(
            s => s.TenantId == tenantId && s.Id == serviceId,
            cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
