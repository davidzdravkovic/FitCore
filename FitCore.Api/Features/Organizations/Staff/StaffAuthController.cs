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
        var (response, error) = await staffAuthService.ActivateAsync(
            request,
            cancellationToken);

        if (error is not null)
            return BadRequest(new { message = error });

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<StaffSessionResponse>> Login(
        [FromBody] LoginStaffRequest request,
        CancellationToken cancellationToken)
    {
        var (response, error) = await staffAuthService.LoginAsync(
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
