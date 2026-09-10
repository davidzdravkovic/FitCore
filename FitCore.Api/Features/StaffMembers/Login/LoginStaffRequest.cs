using System.ComponentModel.DataAnnotations;

namespace FitCore.Api.Features.StaffMembers.Login;

public record LoginStaffRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password);
