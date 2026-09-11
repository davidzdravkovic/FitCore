using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FitCore.Api.Features.Organizations.Admin.Staff.Create;
using FitCore.Api.Features.Organizations.Admin.Staff.Invite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Organizations.Admin.Staff;

[ApiController]
[Route("api/staff")]
[Authorize(Roles = "TenantOwner")]
public class StaffController(StaffService staffService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StaffResponse>>> List(
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized(new { message = "Missing tenant context." });

        var staff = await staffService.ListAsync(tenantId, cancellationToken);
        return Ok(staff);
    }

    [HttpPost]
    public async Task<ActionResult<StaffResponse>> Create(
        [FromBody] CreateStaffRequest request,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized(new { message = "Missing tenant context." });

        var (response, error) = await staffService.CreateAsync(
            tenantId,
            request,
            cancellationToken);

        if (error is not null)
            return BadRequest(new { message = error });

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized(new { message = "Missing tenant context." });

        Guid? actingStaffId = null;
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(sub, out var parsedSub))
            actingStaffId = parsedSub;

        var (ok, error) = await staffService.SoftDeleteAsync(
            tenantId,
            id,
            actingStaffId,
            cancellationToken);

        if (!ok)
        {
            if (error is "You cannot delete your own staff account.")
                return BadRequest(new { message = error });

            return NotFound(new { message = error });
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/invite")]
    public async Task<ActionResult<InviteStaffResponse>> Invite(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized(new { message = "Missing tenant context." });

        var (ok, error) = await staffService.InviteAsync(
            tenantId,
            id,
            cancellationToken);

        if (!ok)
        {
            if (error is "Staff member not found.")
                return NotFound(new { message = error });

            return BadRequest(new { message = error });
        }

        return Ok(new InviteStaffResponse("Invitation sent"));
    }
}
