namespace FitCore.Api.Features.Organizations.Admin.Plans.Create;

public record PlanResponse(
    Guid Id,
    Guid ServiceId,
    string Name,
    decimal Price,
    string EntitlementType,
    int? SessionCount,
    int? DurationDays,
    bool IsActive,
    DateTime CreatedAt);
