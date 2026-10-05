using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Visits;
using FitCore.Api.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Organizations.Members.Me;

[ApiController]
[Route("api/members/me")]
[Authorize(Roles = "Member")]
[RequireActiveTenant]
public class MemberMeController(
    MemberMeService memberMeService,
    ITenantContext tenantContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MemberProfileResponse>> Get(
        CancellationToken cancellationToken)
    {
        if (!TryMemberId(out var memberId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var result = await memberMeService.GetAsync(
            tenantContext.TenantId,
            memberId,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(result.Value);
    }

    [HttpGet("visits")]
    public async Task<ActionResult<IReadOnlyList<VisitResponse>>> ListVisits(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        if (!TryMemberId(out var memberId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var visits = await memberMeService.ListVisitsAsync(
            tenantContext.TenantId,
            memberId,
            from,
            to,
            cancellationToken);
        return Ok(visits);
    }

    private bool TryMemberId(out Guid memberId)
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out memberId);
    }
}
