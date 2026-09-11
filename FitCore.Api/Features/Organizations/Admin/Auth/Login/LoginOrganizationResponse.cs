namespace FitCore.Api.Features.Organizations.Admin.Auth.Login;

public record LoginOrganizationResponse(
    string Message,
    string AccessToken,
    string OrganizationName,
    string OwnerFirstName);
