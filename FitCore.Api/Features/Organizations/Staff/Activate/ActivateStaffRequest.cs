using System.ComponentModel.DataAnnotations;

namespace FitCore.Api.Features.Organizations.Staff.Activate;

public record ActivateStaffRequest(
    [Required] string Token,
    [Required, MinLength(8)] string Password);
