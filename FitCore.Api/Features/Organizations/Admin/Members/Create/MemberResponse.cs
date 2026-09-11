namespace FitCore.Api.Features.Organizations.Admin.Members.Create;

public record MemberResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string Status,
    DateTime CreatedAt);
