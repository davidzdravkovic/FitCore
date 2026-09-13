using System.Security.Claims;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Services.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Organizations.Admin.Services;

[ApiController]
[Route("api/services")]
[Authorize(Roles = "TenantOwner")]
public class ServicesController(GymServiceService gymServiceService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServiceResponse>>> List(
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var services = await gymServiceService.ListAsync(tenantId, cancellationToken);
        return Ok(services);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponse>> Create(
        [FromBody] CreateServiceRequest request,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var result = await gymServiceService.CreateAsync(tenantId, request, cancellationToken);

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

        var result = await gymServiceService.DeactivateAsync(tenantId, id, cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return NoContent();
    }
}
