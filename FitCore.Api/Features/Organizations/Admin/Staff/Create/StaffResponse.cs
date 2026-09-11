namespace FitCore.Api.Features.Organizations.Admin.Staff.Create;

public record StaffResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email);
