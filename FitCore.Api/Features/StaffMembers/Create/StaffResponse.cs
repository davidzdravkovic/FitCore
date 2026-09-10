namespace FitCore.Api.Features.StaffMembers.Create;

public record StaffResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email);
