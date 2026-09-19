namespace FitCore.Api.Domain.Visits;

public static class VisitTransitions
{
    public static void CancelOpen(Visit visit)
    {
        visit.Status = VisitStatus.Cancelled;
    }

    public static void Void(
        Visit visit,
        string? note,
        Guid voidedByStaffId,
        DateTime voidedAtUtc)
    {
        visit.Status = VisitStatus.Voided;
        visit.VoidNote = note;
        visit.VoidedAt = voidedAtUtc;
        visit.VoidedByStaffId = voidedByStaffId;
    }

    public static void Resolve(Visit visit, VisitStatus outcome)
    {
        if (outcome is not (VisitStatus.Completed or VisitStatus.NoShow))
        {
            throw new ArgumentOutOfRangeException(
                nameof(outcome),
                outcome,
                "Resolve outcome must be Completed or NoShow.");
        }

        visit.Status = outcome;
    }

    public static void Reschedule(
        Visit visit,
        Guid coachStaffId,
        DateTime startAtUtc,
        DateTime endAtUtc)
    {
        visit.CoachStaffId = coachStaffId;
        visit.StartAt = startAtUtc;
        visit.EndAt = endAtUtc;
    }
}
