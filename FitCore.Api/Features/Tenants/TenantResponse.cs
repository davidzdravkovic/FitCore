namespace FitCore.Api.Features.Tenants;

public record TenantResponse(
    Guid Id,
    string Name,
    string BusinessEmail,
    string Country,
    string City,
    string TimeZone,
    string Status,
    DateTime CreatedAt);
