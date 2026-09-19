namespace FitCore.Api.Features.Organizations.Admin.Members.Cancel;

public record UnresolvedMembershipResponse(
    Guid Id,
    Guid PlanId,
    string PlanName,
    string Status,
    DateTime StartAt,
    int SessionTotal,
    int SessionsReserved,
    int SessionsBurned,
    int SessionsAvailable);
