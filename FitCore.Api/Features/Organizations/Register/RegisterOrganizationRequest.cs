namespace FitCore.Api.Features.Organizations.Register;

public record RegisterOrganizationRequest(
    string OrganizationName,
    string BusinessEmail,
    string Country,
    string City,
    string TimeZone,
    string OwnerFirstName,
    string OwnerLastName,
    string OwnerEmail,
    string OwnerPassword);
