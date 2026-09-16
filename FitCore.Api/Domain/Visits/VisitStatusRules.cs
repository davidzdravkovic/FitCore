namespace FitCore.Api.Domain.Visits;

public static class VisitStatusRules
{
    /// <summary>Open calendar slots that still hold the coach and (for packs) a reserved credit.</summary>
    public static readonly VisitStatus[] Open =
    [
        VisitStatus.Scheduled,
    ];

    /// <summary>Only scheduled visits can be voided as admin mistakes.</summary>
    public static readonly VisitStatus[] Voidable =
    [
        VisitStatus.Scheduled,
    ];

    /// <summary>Outcomes that keep a session-pack credit burned.</summary>
    public static readonly VisitStatus[] CreditConsumed =
    [
        VisitStatus.Completed,
        VisitStatus.Cancelled,
    ];

    /// <summary>Outcomes that return a reserved session-pack credit.</summary>
    public static readonly VisitStatus[] CreditRestored =
    [
        VisitStatus.Postponed,
        VisitStatus.Voided,
    ];
}
