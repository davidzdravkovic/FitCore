using System.ComponentModel.DataAnnotations;

namespace FitCore.Api.Features.Organizations.Members.Login;

public record LoginMemberRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password);
