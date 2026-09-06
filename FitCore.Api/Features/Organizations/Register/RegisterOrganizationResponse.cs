namespace FitCore.Api.Features.Organizations.Register;

public record RegisterOrganizationResponse(
    string Message,
    string AccessToken,
    string OrganizationName,
    string OwnerFirstName);
