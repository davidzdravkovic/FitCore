using FitCore.Api.Errors.Business;
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
        var result = await organizationService.RegisterAsync(
            request,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(result.Value);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginOrganizationResponse>> Login(
        [FromBody] LoginOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await organizationService.LoginAsync(
            request,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(result.Value);
    }
}
