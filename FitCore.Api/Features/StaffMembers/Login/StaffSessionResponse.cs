namespace FitCore.Api.Features.StaffMembers.Login;

public record StaffSessionResponse(
    string Message,
    string AccessToken,
    string OrganizationName,
    string FirstName);
