using System.ComponentModel.DataAnnotations;
using FitCore.Api.Domain.Memberships;

namespace FitCore.Api.Features.Organizations.Admin.Memberships.Cancel;

public record CancelMembershipRequest(
    [Required] string Reason,
    string? Note = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Reason)
            || !Enum.TryParse<MembershipCancelReason>(Reason.Trim(), ignoreCase: true, out _))
        {
            yield return new ValidationResult(
                "Reason must be MemberRequest or AdminDecision.",
                [nameof(Reason)]);
        }

        var note = Note?.Trim();
        if (note is { Length: > 500 })
        {
            yield return new ValidationResult(
                "Note must be at most 500 characters.",
                [nameof(Note)]);
        }
    }
}
