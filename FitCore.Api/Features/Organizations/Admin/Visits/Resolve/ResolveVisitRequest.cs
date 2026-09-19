namespace FitCore.Api.Features.Organizations.Admin.Visits.Resolve;

/// <summary>Terminal burn outcomes for a scheduled visit.</summary>
public enum VisitResolveOutcome
{
    Completed,
    NoShow,
}

/// <summary>
/// Client "now" in UTC gates resolve-before-start; not stored.
/// Outcome chooses Completed vs NoShow (same pack burn path).
/// </summary>
public sealed record ResolveVisitRequest(VisitResolveOutcome Outcome, DateTime OccurredAt);
