using FitCore.Api.Data.Stores.Platform;
using FitCore.Api.Domain.Enums;
using FitCore.Api.Errors.Business;

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

    public async Task<Result> CancelAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var tenant = await tenantStore.FindByIdAsync(tenantId, cancellationToken);

        if (tenant is null)
            return Result.Fail(ErrorCodes.OrganizationNotFound);

        if (tenant.Status == TenantStatus.Cancelled)
            return Result.Fail(ErrorCodes.OrganizationAlreadyCancelled);

        tenant.Status = TenantStatus.Cancelled;
        await tenantStore.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
