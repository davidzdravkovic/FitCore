using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Services.Create;
using FitCore.Api.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Organizations.Admin.Services;

[ApiController]
[Route("api/services")]
[Authorize(Roles = "TenantOwner")]
[RequireActiveTenant]
public class ServicesController(GymServiceService gymServiceService, ITenantContext tenantContext)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServiceResponse>>> List(
        CancellationToken cancellationToken)
    {
        var services = await gymServiceService.ListAsync(tenantContext.TenantId, cancellationToken);
        return Ok(services);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponse>> Create(
        [FromBody] CreateServiceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await gymServiceService.CreateAsync(
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
        var result = await gymServiceService.DeactivateAsync(
            tenantContext.TenantId,
            id,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return NoContent();
    }
}
