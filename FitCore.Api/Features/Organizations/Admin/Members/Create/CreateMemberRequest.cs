using System.ComponentModel.DataAnnotations;
using FitCore.Api.Domain.Enums;

namespace FitCore.Api.Features.Organizations.Admin.Members.Create;

public record CreateMemberRequest(
    [Required] string FirstName,
    [Required] string LastName,
    [Required] string Status,
    string? Email = null,
    string? Phone = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var email = Email?.Trim();
        var phone = Phone?.Trim();

        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phone))
        {
            yield return new ValidationResult(
                "Provide an email or phone number.",
                [nameof(Email), nameof(Phone)]);
        }

        if (!string.IsNullOrEmpty(email) && !new EmailAddressAttribute().IsValid(email))
        {
            yield return new ValidationResult(
                "Enter a valid email.",
                [nameof(Email)]);
        }

        if (string.IsNullOrWhiteSpace(Status)
            || !Enum.TryParse<MemberStatus>(Status.Trim(), ignoreCase: true, out var status)
            || status is not (MemberStatus.Lead or MemberStatus.Active))
        {
            yield return new ValidationResult(
                "Status must be Lead or Active.",
                [nameof(Status)]);
        }
    }
}
