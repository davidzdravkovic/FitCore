using System.ComponentModel.DataAnnotations;

namespace FitCore.Api.Features.StaffMembers.Create;

public record CreateStaffRequest(
    [Required] string FirstName,
    [Required] string LastName,
    [Required, EmailAddress] string Email);
