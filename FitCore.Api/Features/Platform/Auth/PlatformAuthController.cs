using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Platform.Auth.Login;
using FitCore.Api.Features.Platform.Auth.Verify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Platform.Auth;

[ApiController]
[Route("api/platform-auth")]
public class PlatformAuthController(PlatformAuthService platformAuth) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await platformAuth.ValidateCredentialsAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        await platformAuth.SendLoginMagicLinkAsync(result.Value!, cancellationToken);

        return Ok(new LoginResponse("Check your email"));
    }

    [AllowAnonymous]
    [HttpPost("verify")]
    public async Task<ActionResult<VerifyResponse>> Verify(
        [FromBody] VerifyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await platformAuth.VerifyLoginTokenAsync(
            request.Token,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(new VerifyResponse(result.Value!));
    }
}
