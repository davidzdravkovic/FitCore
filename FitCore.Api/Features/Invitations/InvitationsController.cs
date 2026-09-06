using FitCore.Api.Features.Invitations.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCore.Api.Features.Invitations;

[ApiController]
[Route("api/invitations")]
public class InvitationsController(InvitationService invitationService) : ControllerBase
{
    [Authorize(Roles = "PlatformAdmin")]
    [HttpPost]
    public async Task<ActionResult<CreateInviteResponse>> Invite(
        [FromBody] CreateInviteRequest request,
        CancellationToken cancellationToken)
    {
        await invitationService.CreateInviteAsync(request.Email, cancellationToken);

        return Ok(new CreateInviteResponse("Invitation sent"));
    }
}
