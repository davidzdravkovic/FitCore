using FitCore.Api.Data.Stores.Platform;
using FitCore.Api.Domain.Enums;

namespace FitCore.Api.Features.Platform.Tenants;

public class TenantService(ITenantStore tenantStore)
{
    public async Task<IReadOnlyList<TenantResponse>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var tenants = await tenantStore.ListOrderedByCreatedAtAsync(cancellationToken);

        return tenants
            .Select(t => new TenantResponse(
                t.Id,
                t.Name,
                t.BusinessEmail,
                t.Country,
                t.City,
                t.TimeZone,
                t.Status.ToString(),
                t.CreatedAt))
            .ToList();
    }

    public async Task<(bool Ok, string? Error)> CancelAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var tenant = await tenantStore.FindByIdAsync(tenantId, cancellationToken);

        if (tenant is null)
            return (false, "Organization not found.");

        if (tenant.Status == TenantStatus.Cancelled)
            return (false, "This organization is already cancelled.");

        tenant.Status = TenantStatus.Cancelled;
        await tenantStore.SaveChangesAsync(cancellationToken);

        return (true, null);
    }
}
