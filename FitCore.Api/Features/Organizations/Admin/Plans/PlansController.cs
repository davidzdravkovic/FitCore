using System.Security.Claims;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Plans.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Organizations.Admin.Plans;

[ApiController]
[Route("api/plans")]
[Authorize(Roles = "TenantOwner")]
public class PlansController(PlanService planService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PlanResponse>>> List(
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var plans = await planService.ListAsync(tenantId, cancellationToken);
        return Ok(plans);
    }

    [HttpPost]
    public async Task<ActionResult<PlanResponse>> Create(
        [FromBody] CreatePlanRequest request,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var result = await planService.CreateAsync(tenantId, request, cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var result = await planService.DeactivateAsync(tenantId, id, cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return NoContent();
    }
}
