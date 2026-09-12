using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FitCore.Api.Errors.Business;
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
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

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
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var result = await staffService.CreateAsync(
            tenantId,
            request,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        Guid? actingStaffId = null;
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(sub, out var parsedSub))
            actingStaffId = parsedSub;

        var result = await staffService.SoftDeleteAsync(
            tenantId,
            id,
            actingStaffId,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return NoContent();
    }

    [HttpPost("{id:guid}/invite")]
    public async Task<ActionResult<InviteStaffResponse>> Invite(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var result = await staffService.InviteAsync(
            tenantId,
            id,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(new InviteStaffResponse("Invitation sent"));
    }
}
