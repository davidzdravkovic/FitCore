using FitCore.Api.Data.Stores.OrganizationOwner.PlansStore;
using FitCore.Api.Domain.Plans;
using FitCore.Api.Domain.Tenants;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Plans.Create;

namespace FitCore.Api.Features.Organizations.Admin.Plans;

public class PlanService(IPlanStore planStore)
{
    public async Task<IReadOnlyList<PlanResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var plans = await planStore.ListByTenantAsync(tenantId, cancellationToken);
        return plans.Select(ToResponse).ToList();
    }

    public async Task<Result<PlanResponse>> CreateAsync(
        Guid tenantId,
        CreatePlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenant = await planStore.FindTenantByIdAsync(tenantId, cancellationToken);

        if (tenant is null)
            return Result<PlanResponse>.Fail(ErrorCodes.OrganizationNotFound);

        if (tenant.Status != TenantStatus.Active)
            return Result<PlanResponse>.Fail(ErrorCodes.OrganizationNotActive);

        var service = await planStore.FindActiveServiceAsync(
            tenantId,
            request.ServiceId,
            cancellationToken);

        if (service is null)
            return Result<PlanResponse>.Fail(ErrorCodes.ServiceNotFound);

        var name = request.Name.Trim();
        if (await planStore.NameTakenAsync(tenantId, name, cancellationToken))
            return Result<PlanResponse>.Fail(ErrorCodes.PlanNameTaken);

        var entitlement = Enum.Parse<PlanEntitlementType>(
            request.EntitlementType.Trim(),
            ignoreCase: true);

        var plan = new MembershipPlan
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ServiceId = service.Id,
            Name = name,
            Price = request.Price,
            EntitlementType = entitlement,
            SessionCount = entitlement == PlanEntitlementType.SessionPack
                ? request.SessionCount
                : null,
            DurationDays = entitlement == PlanEntitlementType.TimePeriod
                ? request.DurationDays
                : null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        await planStore.AddAsync(plan);
        await planStore.SaveChangesAsync(cancellationToken);

        return Result<PlanResponse>.Success(ToResponse(plan));
    }

    public async Task<Result> DeactivateAsync(
        Guid tenantId,
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        var plan = await planStore.FindByIdAsync(tenantId, planId, cancellationToken);

        if (plan is null)
            return Result.Fail(ErrorCodes.PlanNotFound);

        if (!plan.IsActive)
            return Result.Success();

        plan.IsActive = false;
        await planStore.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static PlanResponse ToResponse(MembershipPlan plan) =>
        new(
            plan.Id,
            plan.ServiceId,
            plan.Name,
            plan.Price,
            plan.EntitlementType.ToString(),
            plan.SessionCount,
            plan.DurationDays,
            plan.IsActive,
            plan.CreatedAt);
}
