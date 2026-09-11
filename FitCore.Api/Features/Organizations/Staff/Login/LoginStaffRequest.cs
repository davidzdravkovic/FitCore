using System.ComponentModel.DataAnnotations;

namespace FitCore.Api.Features.Organizations.Staff.Login;

public record LoginStaffRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password);
