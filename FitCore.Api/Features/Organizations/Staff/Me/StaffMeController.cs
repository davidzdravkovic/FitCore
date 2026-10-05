using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Members;
using FitCore.Api.Features.Organizations.Admin.Members.Create;
using FitCore.Api.Features.Organizations.Admin.Memberships;
using FitCore.Api.Features.Organizations.Admin.Memberships.Assign;
using FitCore.Api.Features.Organizations.Admin.Staff.Create;
using FitCore.Api.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Organizations.Staff.Me;

[ApiController]
[Route("api/staff/me")]
[Authorize(Roles = "TenantStaff")]
[RequireActiveTenant]
public class StaffMeController(
    StaffMeService staffMeService,
    MemberService memberService,
    MembershipService membershipService,
    ITenantContext tenantContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<StaffResponse>> Get(CancellationToken cancellationToken)
    {
        if (!TryStaffId(out var staffId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var result = await staffMeService.GetAsync(
            tenantContext.TenantId,
            staffId,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(result.Value);
    }

    [HttpGet("members")]
    public async Task<ActionResult<IReadOnlyList<MemberResponse>>> ListMembers(
        CancellationToken cancellationToken)
    {
        var members = await memberService.ListAsync(
            tenantContext.TenantId,
            cancellationToken);
        return Ok(members);
    }

    [HttpGet("memberships")]
    public async Task<ActionResult<IReadOnlyList<MembershipResponse>>> ListMemberships(
        CancellationToken cancellationToken)
    {
        var memberships = await membershipService.ListAsync(
            tenantContext.TenantId,
            cancellationToken);
        return Ok(memberships);
    }

    private bool TryStaffId(out Guid staffId)
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out staffId);
    }
}
