using System.ComponentModel.DataAnnotations;

namespace FitCore.Api.Features.Organizations.Admin.Members.Create;

public record CreateMemberRequest(
    [Required] string FirstName,
    [Required] string LastName,
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

        if (!string.IsNullOrEmpty(phone) && !IsValidPhone(phone))
        {
            yield return new ValidationResult(
                "Enter a valid phone number.",
                [nameof(Phone)]);
        }
    }

    private static bool IsValidPhone(string phone)
    {
        // Digits with optional +, spaces, dashes, parentheses — not emails/text.
        if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\+?[\d\s\-().]+$"))
            return false;

        var digits = System.Text.RegularExpressions.Regex.Replace(phone, @"\D", "");
        return digits.Length is >= 7 and <= 15;
    }
}
