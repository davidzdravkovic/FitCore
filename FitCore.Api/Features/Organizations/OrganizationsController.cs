using FitCore.Api.Features.Organizations.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Organizations;

[ApiController]
[Route("api/organizations")]
public class OrganizationsController : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public ActionResult<RegisterOrganizationResponse> Register([FromBody] RegisterOrganizationRequest request)
    {
        return Ok(new RegisterOrganizationResponse("Not implemented"));
    }
}
