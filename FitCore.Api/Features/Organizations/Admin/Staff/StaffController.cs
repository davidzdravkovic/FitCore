using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Staff.Create;
using FitCore.Api.Features.Organizations.Admin.Staff.Invite;
using FitCore.Api.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Organizations.Admin.Staff;

[ApiController]
[Route("api/staff")]
[Authorize(Roles = "TenantOwner")]
[RequireActiveTenant]
public class StaffController(StaffService staffService, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StaffResponse>>> List(
        CancellationToken cancellationToken)
    {
        var staff = await staffService.ListAsync(tenantContext.TenantId, cancellationToken);
        return Ok(staff);
    }

    [HttpPost]
    public async Task<ActionResult<StaffResponse>> Create(
        [FromBody] CreateStaffRequest request,
        CancellationToken cancellationToken)
    {
        var result = await staffService.CreateAsync(
            tenantContext.TenantId,
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
        Guid? actingStaffId = null;
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(sub, out var parsedSub))
            actingStaffId = parsedSub;

        var result = await staffService.SoftDeleteAsync(
            tenantContext.TenantId,
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
        var result = await staffService.InviteAsync(
            tenantContext.TenantId,
            id,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(new InviteStaffResponse("Invitation sent"));
    }
}
