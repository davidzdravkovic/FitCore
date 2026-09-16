namespace FitCore.Api.Data.Stores.Locking;

/// <summary>
/// Transaction-scoped row locks that enforce
/// Membership → Member → Coach → Visit.
/// </summary>
public interface IOrderedRowLockScope : IAsyncDisposable
{
    Task LockMembershipAsync(
        Guid tenantId,
        Guid membershipId,
        CancellationToken cancellationToken = default);

    Task LockMemberAsync(
        Guid tenantId,
        Guid memberId,
        CancellationToken cancellationToken = default);

    Task LockCoachAsync(
        Guid tenantId,
        Guid coachStaffId,
        CancellationToken cancellationToken = default);

    Task LockVisitAsync(
        Guid tenantId,
        Guid visitId,
        CancellationToken cancellationToken = default);

    Task CommitAsync(CancellationToken cancellationToken = default);
}
