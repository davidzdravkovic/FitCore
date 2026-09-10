namespace FitCore.Api.Features.Members.Create;

public record MemberResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string Status,
    DateTime CreatedAt);
