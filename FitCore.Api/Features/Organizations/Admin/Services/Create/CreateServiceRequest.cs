using System.ComponentModel.DataAnnotations;

namespace FitCore.Api.Features.Organizations.Admin.Services.Create;

public record CreateServiceRequest(
    [Required, MinLength(1), MaxLength(200)] string Name,
    [MaxLength(2000)] string? Description = null);
