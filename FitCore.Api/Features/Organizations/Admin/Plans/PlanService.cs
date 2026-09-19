using FitCore.Api.Data.Stores.OrganizationOwner.PlansStore;
using FitCore.Api.Domain.Plans;
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
        var service = await planStore.FindActiveServiceAsync(
            tenantId,
            request.ServiceId,
            cancellationToken);

        if (service is null)
            return Result<PlanResponse>.Fail(ErrorCodes.ServiceNotFound);

        var name = request.Name.Trim();
        if (await planStore.NameTakenAsync(tenantId, name, cancellationToken))
            return Result<PlanResponse>.Fail(ErrorCodes.PlanNameTaken);

        var plan = new MembershipPlan
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ServiceId = service.Id,
            Name = name,
            Price = request.Price,
            EntitlementType = PlanEntitlementType.SessionPack,
            SessionCount = request.SessionCount,
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
            plan.IsActive,
            plan.CreatedAt);
}
