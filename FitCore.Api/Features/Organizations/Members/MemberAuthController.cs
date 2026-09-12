using FitCore.Api.Errors;
using FitCore.Api.Features.Organizations.Members.Activate;
using FitCore.Api.Features.Organizations.Members.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Organizations.Members;

[ApiController]
[Route("api/members")]
[AllowAnonymous]
public class MemberAuthController(MemberAuthService memberAuthService) : ControllerBase
{
    [HttpPost("activate")]
    public async Task<ActionResult<MemberSessionResponse>> Activate(
        [FromBody] ActivateMemberRequest request,
        CancellationToken cancellationToken)
    {
        var result = await memberAuthService.ActivateAsync(
            request,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<ActionResult<MemberSessionResponse>> Login(
        [FromBody] LoginMemberRequest request,
        CancellationToken cancellationToken)
    {
        var result = await memberAuthService.LoginAsync(
            request,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(result.Value);
    }
}
