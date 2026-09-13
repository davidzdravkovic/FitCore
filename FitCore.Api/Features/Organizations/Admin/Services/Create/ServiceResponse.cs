namespace FitCore.Api.Features.Organizations.Admin.Services.Create;

public record ServiceResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt);
