using FitCore.Api.Features.Organizations.Admin.Auth.Login;
using FitCore.Api.Features.Organizations.Admin.Auth.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Organizations.Admin.Auth;

[ApiController]
[Route("api/organizations")]
public class OrganizationsController(OrganizationService organizationService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<RegisterOrganizationResponse>> Register(
        [FromBody] RegisterOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var (response, error) = await organizationService.RegisterAsync(
            request,
            cancellationToken);

        if (error is not null)
            return BadRequest(new { message = error });

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginOrganizationResponse>> Login(
        [FromBody] LoginOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var (response, error) = await organizationService.LoginAsync(
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
