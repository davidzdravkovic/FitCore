using System.ComponentModel.DataAnnotations;

namespace FitCore.Api.Features.Platform.Auth.Verify;

public record VerifyRequest(
    [Required] string Token);
