namespace FitCore.Api.Features.Organizations.Admin.Visits;

public record VisitResponse(
    Guid Id,
    Guid MembershipId,
    Guid MemberId,
    string MemberName,
    Guid CoachStaffId,
    string CoachName,
    Guid ServiceId,
    string ServiceName,
    DateTime StartAt,
    DateTime EndAt,
    string Status,
    bool ConsumedSessionCredit,
    DateTime CreatedAt,
    string? VoidNote = null,
    DateTime? VoidedAt = null);
