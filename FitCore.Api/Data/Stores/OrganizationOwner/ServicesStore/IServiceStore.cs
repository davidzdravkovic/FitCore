using FitCore.Api.Domain.Services;
using FitCore.Api.Domain.Tenants;

namespace FitCore.Api.Data.Stores.OrganizationOwner.ServicesStore;

public interface IServiceStore
{
    Task<IReadOnlyList<Service>> ListByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<Tenant?> FindTenantByIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<bool> NameTakenAsync(
        Guid tenantId,
        string name,
        CancellationToken cancellationToken = default);

    Task AddAsync(Service service);

    Task<Service?> FindByIdAsync(
        Guid tenantId,
        Guid serviceId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
