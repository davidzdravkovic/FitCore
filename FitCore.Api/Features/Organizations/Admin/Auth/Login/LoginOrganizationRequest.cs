using System.ComponentModel.DataAnnotations;

namespace FitCore.Api.Features.Organizations.Admin.Auth.Login;

public record LoginOrganizationRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password);
