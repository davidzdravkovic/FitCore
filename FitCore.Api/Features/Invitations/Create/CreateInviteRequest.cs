using System.ComponentModel.DataAnnotations;

namespace FitCore.Api.Features.Invitations.Create;

public record CreateInviteRequest(
    [Required, EmailAddress] string Email);
