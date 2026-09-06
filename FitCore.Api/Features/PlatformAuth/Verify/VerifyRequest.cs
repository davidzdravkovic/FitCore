using System.ComponentModel.DataAnnotations;

namespace FitCore.Api.Features.PlatformAuth.Verify;

public record VerifyRequest(
    [Required] string Token);
