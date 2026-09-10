using System.ComponentModel.DataAnnotations;

namespace FitCore.Api.Features.StaffMembers.Activate;

public record ActivateStaffRequest(
    [Required] string Token,
    [Required, MinLength(8)] string Password);
