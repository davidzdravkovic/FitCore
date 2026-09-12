using System.Security.Claims;
using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Members.Create;
using FitCore.Api.Features.Organizations.Admin.Members.Invite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Organizations.Admin.Members;

[ApiController]
[Route("api/members")]
[Authorize(Roles = "TenantOwner")]
public class MembersController(MemberService memberService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MemberResponse>>> List(
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var members = await memberService.ListAsync(tenantId, cancellationToken);
        return Ok(members);
    }

    [HttpPost]
    public async Task<ActionResult<MemberResponse>> Create(
        [FromBody] CreateMemberRequest request,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var result = await memberService.CreateAsync(
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

        var result = await memberService.SoftDeleteAsync(
            tenantId,
            id,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return NoContent();
    }

    [HttpPost("{id:guid}/invite")]
    public async Task<ActionResult<InviteMemberResponse>> Invite(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return ErrorResults.From(ErrorCodes.MissingTenantContext);

        var result = await memberService.InviteAsync(
            tenantId,
            id,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(new InviteMemberResponse("Invitation sent"));
    }
}
