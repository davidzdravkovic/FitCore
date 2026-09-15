using FitCore.Api.Domain.Plans;
using FitCore.Api.Domain.Services;
using FitCore.Api.Domain.Tenants;

namespace FitCore.Api.Data.Stores.OrganizationOwner.PlansStore;

public interface IPlanStore
{
    Task<IReadOnlyList<MembershipPlan>> ListByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<Tenant?> FindTenantByIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<Service?> FindActiveServiceAsync(
        Guid tenantId,
        Guid serviceId,
        CancellationToken cancellationToken = default);

    Task<bool> NameTakenAsync(
        Guid tenantId,
        string name,
        CancellationToken cancellationToken = default);

    Task AddAsync(MembershipPlan plan);

    Task<MembershipPlan?> FindByIdAsync(
        Guid tenantId,
        Guid planId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
