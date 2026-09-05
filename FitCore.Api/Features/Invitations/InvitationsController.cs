using FitCore.Api.Features.Invitations.Create;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Invitations;

[ApiController]
[Route("api/invitations")]
public class InvitationsController : ControllerBase
{
    [HttpPost]
    public ActionResult<CreateInviteResponse> Create([FromBody] CreateInviteRequest request)
    {
        return Ok(new CreateInviteResponse("Not implemented"));
    }
}
