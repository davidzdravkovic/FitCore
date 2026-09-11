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
        var (ok, error) = await tenantService.CancelAsync(id, cancellationToken);

        if (!ok)
        {
            if (error is "Organization not found.")
                return NotFound(new { message = error });

            return BadRequest(new { message = error });
        }

        return NoContent();
    }
}
