using System.ComponentModel.DataAnnotations;
using FitCore.Api.Domain.Plans;

namespace FitCore.Api.Features.Organizations.Admin.Plans.Create;

public record CreatePlanRequest(
    [Required] Guid ServiceId,
    [Required, MinLength(1), MaxLength(200)] string Name,
    [Range(0, double.MaxValue)] decimal Price,
    [Required] string EntitlementType,
    int? SessionCount = null,
    int? DurationDays = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(EntitlementType)
            || !Enum.TryParse<PlanEntitlementType>(
                EntitlementType.Trim(),
                ignoreCase: true,
                out var type))
        {
            yield return new ValidationResult(
                "EntitlementType must be SessionPack or TimePeriod.",
                [nameof(EntitlementType)]);
            yield break;
        }

        if (type == PlanEntitlementType.SessionPack)
        {
            if (SessionCount is null or <= 0)
            {
                yield return new ValidationResult(
                    "SessionCount must be greater than 0 for SessionPack.",
                    [nameof(SessionCount)]);
            }

            if (DurationDays is not null)
            {
                yield return new ValidationResult(
                    "DurationDays must be empty for SessionPack.",
                    [nameof(DurationDays)]);
            }
        }
        else
        {
            if (DurationDays is null or <= 0)
            {
                yield return new ValidationResult(
                    "DurationDays must be greater than 0 for TimePeriod.",
                    [nameof(DurationDays)]);
            }

            if (SessionCount is not null)
            {
                yield return new ValidationResult(
                    "SessionCount must be empty for TimePeriod.",
                    [nameof(SessionCount)]);
            }
        }
    }
}
