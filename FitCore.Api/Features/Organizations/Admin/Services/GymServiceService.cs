using FitCore.Api.Data.Stores.OrganizationOwner.ServicesStore;
using FitCore.Api.Domain.Enums;
using FitCore.Api.Domain.Entities;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Services.Create;

namespace FitCore.Api.Features.Organizations.Admin.Services;

public class GymServiceService(IServiceStore serviceStore)
{
    public async Task<IReadOnlyList<ServiceResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var services = await serviceStore.ListByTenantAsync(tenantId, cancellationToken);

        return services.Select(ToResponse).ToList();
    }

    public async Task<Result<ServiceResponse>> CreateAsync(
        Guid tenantId,
        CreateServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenant = await serviceStore.FindTenantByIdAsync(tenantId, cancellationToken);

        if (tenant is null)
            return Result<ServiceResponse>.Fail(ErrorCodes.OrganizationNotFound);

        if (tenant.Status != TenantStatus.Active)
            return Result<ServiceResponse>.Fail(ErrorCodes.OrganizationNotActive);

        var name = request.Name.Trim();
        if (await serviceStore.NameTakenAsync(tenantId, name, cancellationToken))
            return Result<ServiceResponse>.Fail(ErrorCodes.ServiceNameTaken);

        var service = new Service
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        await serviceStore.AddAsync(service);
        await serviceStore.SaveChangesAsync(cancellationToken);

        return Result<ServiceResponse>.Success(ToResponse(service));
    }

    public async Task<Result> DeactivateAsync(
        Guid tenantId,
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        var service = await serviceStore.FindByIdAsync(tenantId, serviceId, cancellationToken);

        if (service is null)
            return Result.Fail(ErrorCodes.ServiceNotFound);

        if (!service.IsActive)
            return Result.Success();

        service.IsActive = false;
        await serviceStore.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static ServiceResponse ToResponse(Service service) =>
        new(service.Id, service.Name, service.Description, service.IsActive, service.CreatedAt);
}
