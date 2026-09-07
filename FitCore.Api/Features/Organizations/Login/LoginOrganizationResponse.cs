namespace FitCore.Api.Features.Organizations.Login;

public record LoginOrganizationResponse(
    string Message,
    string AccessToken,
    string OrganizationName,
    string OwnerFirstName);
