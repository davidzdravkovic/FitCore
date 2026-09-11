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
        var admin = await platformAuth.ValidateCredentialsAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (admin is null)
            return Unauthorized(new LoginResponse("Invalid email or password"));

        await platformAuth.SendLoginMagicLinkAsync(admin, cancellationToken);

        return Ok(new LoginResponse("Check your email"));
    }

    [AllowAnonymous]
    [HttpPost("verify")]
    public async Task<ActionResult<VerifyResponse>> Verify(
        [FromBody] VerifyRequest request,
        CancellationToken cancellationToken)
    {
        var accessToken = await platformAuth.VerifyLoginTokenAsync(
            request.Token,
            cancellationToken);

        if (accessToken is null)
            return Unauthorized(new { message = "This sign-in link is invalid or has expired." });

        return Ok(new VerifyResponse(accessToken));
    }
}
