using FitCore.Api.Errors.Business;
using FitCore.Api.Features.Organizations.Staff.Activate;
using FitCore.Api.Features.Organizations.Staff.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Organizations.Staff;

[ApiController]
[Route("api/staff")]
[AllowAnonymous]
public class StaffAuthController(StaffAuthService staffAuthService) : ControllerBase
{
    [HttpPost("activate")]
    public async Task<ActionResult<StaffSessionResponse>> Activate(
        [FromBody] ActivateStaffRequest request,
        CancellationToken cancellationToken)
    {
        var result = await staffAuthService.ActivateAsync(
            request,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<ActionResult<StaffSessionResponse>> Login(
        [FromBody] LoginStaffRequest request,
        CancellationToken cancellationToken)
    {
        var result = await staffAuthService.LoginAsync(
            request,
            cancellationToken);

        if (!result.Succeeded)
            return ErrorResults.From(result.Error!);

        return Ok(result.Value);
    }
}
