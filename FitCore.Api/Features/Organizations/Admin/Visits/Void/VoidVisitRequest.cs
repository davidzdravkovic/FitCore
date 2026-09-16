using System.ComponentModel.DataAnnotations;

namespace FitCore.Api.Features.Organizations.Admin.Visits.Void;

public record VoidVisitRequest(string? Note = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var note = Note?.Trim();
        if (note is { Length: > 500 })
        {
            yield return new ValidationResult(
                "Note must be at most 500 characters.",
                [nameof(Note)]);
        }
    }
}
