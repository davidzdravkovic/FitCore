namespace FitCore.Api.Features.Organizations.Admin.Memberships.Assign;

public record MembershipResponse(
    Guid Id,
    Guid MemberId,
    string MemberName,
    Guid PlanId,
    string PlanName,
    string Status,
    DateTime StartAt,
    int SessionTotal,
    int SessionsReserved,
    int SessionsBurned,
    int SessionsAvailable,
    DateTime CreatedAt,
    string? CancelReason = null,
    string? CancelNote = null,
    DateTime? CancelledAt = null);
