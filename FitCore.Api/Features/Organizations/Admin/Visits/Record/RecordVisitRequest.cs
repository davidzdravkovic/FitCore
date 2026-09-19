using System.ComponentModel.DataAnnotations;
using FitCore.Api.Features.Organizations.Admin.Visits.Resolve;

namespace FitCore.Api.Features.Organizations.Admin.Visits.Record;

/// <summary>
/// Backfill a past visit that was never scheduled — creates Completed or NoShow and burns one credit.
/// </summary>
public record RecordVisitRequest(
    [Required] Guid MembershipId,
    [Required] Guid CoachStaffId,
    [Required] DateTime StartAt,
    [Required] DateTime EndAt,
    [Required] VisitResolveOutcome Outcome) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndAt <= StartAt)
        {
            yield return new ValidationResult(
                "EndAt must be after StartAt.",
                [nameof(EndAt)]);
        }

        if (Outcome is not (VisitResolveOutcome.Completed or VisitResolveOutcome.NoShow))
        {
            yield return new ValidationResult(
                "Outcome must be Completed or NoShow.",
                [nameof(Outcome)]);
        }
    }
}
