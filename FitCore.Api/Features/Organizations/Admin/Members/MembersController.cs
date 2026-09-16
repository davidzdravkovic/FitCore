using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Admin.Members.Create;
using FitCore.Api.Features.Organizations.Admin.Members.Invite;
using FitCore.Api.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Organizations.Admin.Members;

[ApiController]
[Route("api/members")]
[Authorize(Roles = "TenantOwner")]
[RequireActiveTenant]
public class MembersController(MemberService memberService, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MemberResponse>>> List(
        CancellationToken cancellationToken)
    {
        var members = await memberService.ListAsync(tenantContext.TenantId, cancellationToken);
        return Ok(members);
    }

    [HttpPost]
    public async Task<ActionResult<MemberResponse>> Create(
        [FromBody] CreateMemberRequest request,
        CancellationToken cancellationToken)
    {
        var result = await memberService.CreateAsync(
            tenantContext.TenantId,
            request,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(result.Value);
    }

    [HttpPost("import")]
    public async Task<ActionResult<MemberResponse>> Import(
        [FromBody] CreateMemberRequest request,
        CancellationToken cancellationToken)
    {
        var result = await memberService.ImportAsync(
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
        var result = await memberService.CancelAsync(
            tenantContext.TenantId,
            id,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!, result.Details);

        return NoContent();
    }

    [HttpPost("{id:guid}/invite")]
    public async Task<ActionResult<InviteMemberResponse>> Invite(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await memberService.InviteAsync(
            tenantContext.TenantId,
            id,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(new InviteMemberResponse("Invitation sent"));
    }
}
