namespace FitCore.Api.Domain.Visits;

public enum VisitStatus
{
    /// <summary>Upcoming visit; session-pack credit is reserved.</summary>
    Scheduled,

    /// <summary>Check-in / delivered; credit stays consumed.</summary>
    Completed,

    /// <summary>Early release / reschedule path; credit restored.</summary>
    Postponed,

    /// <summary>Late cancel / forfeit; credit stays consumed.</summary>
    Cancelled,

    /// <summary>Created in error; credit restored; excluded from burn/delivery stats.</summary>
    Voided
}
