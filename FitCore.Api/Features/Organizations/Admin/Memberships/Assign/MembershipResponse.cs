namespace FitCore.Api.Features.Organizations.Admin.Memberships.Assign;

public record MembershipResponse(
    Guid Id,
    Guid MemberId,
    string MemberName,
    Guid PlanId,
    string PlanName,
    string Status,
    DateTime StartAt,
    DateTime? EndAt,
    int? SessionsRemaining,
    DateTime CreatedAt);
