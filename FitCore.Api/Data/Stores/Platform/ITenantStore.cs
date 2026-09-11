using FitCore.Api.Domain.Entities;

namespace FitCore.Api.Data.Stores.Platform;

public interface ITenantStore
{
    Task<IReadOnlyList<Tenant>> ListOrderedByCreatedAtAsync(
        CancellationToken cancellationToken = default);

    Task<Tenant?> FindByIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
