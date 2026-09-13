using System.Security.Claims;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Memberships.Assign;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Organizations.Admin.Memberships;

[ApiController]
[Route("api/memberships")]
[Authorize(Roles = "TenantOwner")]
public class MembershipsController(MembershipService membershipService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MembershipResponse>>> List(
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var memberships = await membershipService.ListAsync(tenantId, cancellationToken);
        return Ok(memberships);
    }

    [HttpPost]
    public async Task<ActionResult<MembershipResponse>> Assign(
        [FromBody] AssignMembershipRequest request,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var result = await membershipService.AssignAsync(tenantId, request, cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(result.Value);
    }
}
