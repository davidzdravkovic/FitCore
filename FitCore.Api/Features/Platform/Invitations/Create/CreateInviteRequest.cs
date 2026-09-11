using System.ComponentModel.DataAnnotations;

namespace FitCore.Api.Features.Platform.Invitations.Create;

public record CreateInviteRequest(
    [Required, EmailAddress] string Email);
