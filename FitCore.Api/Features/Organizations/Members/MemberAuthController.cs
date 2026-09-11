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
        var (response, error) = await memberAuthService.ActivateAsync(
            request,
            cancellationToken);

        if (error is not null)
            return BadRequest(new { message = error });

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<MemberSessionResponse>> Login(
        [FromBody] LoginMemberRequest request,
        CancellationToken cancellationToken)
    {
        var (response, error) = await memberAuthService.LoginAsync(
            request,
            cancellationToken);

        if (error is not null)
        {
            if (error == "Invalid email or password")
                return Unauthorized(new { message = error });

            return BadRequest(new { message = error });
        }

        return Ok(response);
    }
}
