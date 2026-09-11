using FitCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitCore.Api.Data.Stores.Platform;

public class EfTenantStore(AppDbContext db) : ITenantStore
{
    public async Task<IReadOnlyList<Tenant>> ListOrderedByCreatedAtAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.Tenants
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Tenant?> FindByIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return db.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
