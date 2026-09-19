namespace FitCore.Api.Domain.Visits;

public static class VisitStatusRules
{
    /// <summary>Open calendar slots that still hold the coach and (for packs) a reserved credit.</summary>
    public static readonly VisitStatus[] Open =
    [
        VisitStatus.Scheduled,
    ];

    /// <summary>
    /// Statuses that block the same member/coach time window — including backfilled
    /// Completed/NoShow so Record cannot stack two outcomes on one slot.
    /// </summary>
    public static readonly VisitStatus[] OccupiesSlot =
    [
        VisitStatus.Scheduled,
        VisitStatus.Completed,
        VisitStatus.NoShow,
    ];

    /// <summary>Only scheduled visits can be voided as admin mistakes.</summary>
    public static readonly VisitStatus[] Voidable =
    [
        VisitStatus.Scheduled,
    ];

    /// <summary>Scheduled visits that can be resolved to Completed or NoShow.</summary>
    public static readonly VisitStatus[] Resolvable =
    [
        VisitStatus.Scheduled,
    ];

    /// <summary>Only scheduled visits can be moved to a new time.</summary>
    public static readonly VisitStatus[] Reschedulable =
    [
        VisitStatus.Scheduled,
    ];

    /// <summary>Outcomes that keep a session-pack credit burned.</summary>
    public static readonly VisitStatus[] CreditConsumed =
    [
        VisitStatus.Completed,
        VisitStatus.NoShow,
        VisitStatus.Cancelled,
    ];

    /// <summary>Outcomes that return a reserved session-pack credit.</summary>
    public static readonly VisitStatus[] CreditRestored =
    [
        VisitStatus.Postponed,
        VisitStatus.Voided,
    ];
}
