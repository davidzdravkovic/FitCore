using FitCore.Api.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Platform.Tenants;

[ApiController]
[Route("api/tenants")]
[Authorize(Roles = "PlatformAdmin")]
public class TenantsController(TenantService tenantService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TenantResponse>>> List(
        CancellationToken cancellationToken)
    {
        var tenants = await tenantService.ListAsync(cancellationToken);
        return Ok(tenants);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await tenantService.CancelAsync(id, cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return NoContent();
    }
}
