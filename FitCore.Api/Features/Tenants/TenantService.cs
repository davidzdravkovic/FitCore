using FitCore.Api.Data;
using FitCore.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace FitCore.Api.Features.Tenants;

public class TenantService(AppDbContext db)
{
    public async Task<IReadOnlyList<TenantResponse>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.Tenants
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TenantResponse(
                t.Id,
                t.Name,
                t.BusinessEmail,
                t.Country,
                t.City,
                t.TimeZone,
                t.Status.ToString(),
                t.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<(bool Ok, string? Error)> CancelAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var tenant = await db.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant is null)
            return (false, "Organization not found.");

        if (tenant.Status == TenantStatus.Cancelled)
            return (false, "This organization is already cancelled.");

        tenant.Status = TenantStatus.Cancelled;
        await db.SaveChangesAsync(cancellationToken);

        return (true, null);
    }
}
