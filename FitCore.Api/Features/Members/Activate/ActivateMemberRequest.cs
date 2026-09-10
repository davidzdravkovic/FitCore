using System.ComponentModel.DataAnnotations;

namespace FitCore.Api.Features.Members.Activate;

public record ActivateMemberRequest(
    [Required] string Token,
    [Required, MinLength(8)] string Password);
