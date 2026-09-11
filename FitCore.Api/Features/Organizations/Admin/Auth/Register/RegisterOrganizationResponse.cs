namespace FitCore.Api.Features.Organizations.Admin.Auth.Register;

public record RegisterOrganizationResponse(
    string Message,
    string AccessToken,
    string OrganizationName,
    string OwnerFirstName);
