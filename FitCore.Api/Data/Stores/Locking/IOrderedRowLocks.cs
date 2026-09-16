namespace FitCore.Api.Data.Stores.Locking;

public interface IOrderedRowLocks
{
    Task<IOrderedRowLockScope> BeginAsync(
        CancellationToken cancellationToken = default);
}
