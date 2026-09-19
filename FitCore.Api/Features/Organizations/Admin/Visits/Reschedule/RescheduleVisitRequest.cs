using System.ComponentModel.DataAnnotations;

namespace FitCore.Api.Features.Organizations.Admin.Visits.Reschedule;

public record RescheduleVisitRequest(
    [Required] Guid CoachStaffId,
    [Required] DateTime StartAt,
    [Required] DateTime EndAt) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndAt <= StartAt)
        {
            yield return new ValidationResult(
                "EndAt must be after StartAt.",
                [nameof(EndAt)]);
        }
    }
}
