namespace FitCore.Api.Features.Organizations.Staff.Login;

public record StaffSessionResponse(
    string Message,
    string AccessToken,
    string OrganizationName,
    string FirstName);
