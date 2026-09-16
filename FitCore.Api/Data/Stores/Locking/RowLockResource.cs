namespace FitCore.Api.Data.Stores.Locking;

/// <summary>
/// Global row-lock order. Acquire only forward; skipping is allowed. See
/// <c>docs/architecture/row-lock-ordering.md</c>.
/// </summary>
public enum RowLockResource
{
    None = 0,
    Membership = 1,
    Member = 2,
    Coach = 3,
    Visit = 4,
}
