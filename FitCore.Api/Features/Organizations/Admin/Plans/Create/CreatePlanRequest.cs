using System.ComponentModel.DataAnnotations;
using FitCore.Api.Domain.Plans;

namespace FitCore.Api.Features.Organizations.Admin.Plans.Create;

public record CreatePlanRequest(
    [Required] Guid ServiceId,
    [Required, MinLength(1), MaxLength(200)] string Name,
    [Range(0, double.MaxValue)] decimal Price,
    [Required] string EntitlementType,
    [Required] int SessionCount) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(EntitlementType)
            || !Enum.TryParse<PlanEntitlementType>(
                EntitlementType.Trim(),
                ignoreCase: true,
                out var type)
            || type != PlanEntitlementType.SessionPack)
        {
            yield return new ValidationResult(
                "EntitlementType must be SessionPack.",
                [nameof(EntitlementType)]);
        }

        if (SessionCount <= 0)
        {
            yield return new ValidationResult(
                "SessionCount must be greater than 0.",
                [nameof(SessionCount)]);
        }
    }
}
