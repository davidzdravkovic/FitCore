using System.ComponentModel.DataAnnotations;

namespace FitCore.Api.Features.Organizations.Admin.Staff.Create;

public record CreateStaffRequest(
    [Required] string FirstName,
    [Required] string LastName,
    [Required, EmailAddress] string Email);
