using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Plans.Create;
using FitCore.Api.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Organizations.Admin.Plans;

[ApiController]
[Route("api/plans")]
[Authorize(Roles = "TenantOwner")]
[RequireActiveTenant]
public class PlansController(PlanService planService, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PlanResponse>>> List(
        CancellationToken cancellationToken)
    {
        var plans = await planService.ListAsync(tenantContext.TenantId, cancellationToken);
        return Ok(plans);
    }

    [HttpPost]
    public async Task<ActionResult<PlanResponse>> Create(
        [FromBody] CreatePlanRequest request,
        CancellationToken cancellationToken)
    {
        var result = await planService.CreateAsync(
            tenantContext.TenantId,
            request,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await planService.DeactivateAsync(
            tenantContext.TenantId,
            id,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return NoContent();
    }
}
