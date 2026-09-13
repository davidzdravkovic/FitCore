namespace FitCore.Api.Features.Platform.Tenants;

public record TenantResponse(
    Guid Id,
    string Name,
    string BusinessEmail,
    string Country,
    string City,
    string TimeZone,
    string Currency,
    string Status,
    DateTime CreatedAt);
