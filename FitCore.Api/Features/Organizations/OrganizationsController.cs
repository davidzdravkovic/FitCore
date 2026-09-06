using FitCore.Api.Features.Organizations.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Organizations;

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
}
