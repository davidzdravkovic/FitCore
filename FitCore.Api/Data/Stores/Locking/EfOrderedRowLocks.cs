using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FitCore.Api.Data.Stores.Locking;

public sealed class EfOrderedRowLocks(AppDbContext db) : IOrderedRowLocks
{
    public async Task<IOrderedRowLockScope> BeginAsync(
        CancellationToken cancellationToken = default)
    {
        var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        return new Scope(db, transaction);
    }

    private sealed class Scope(AppDbContext db, IDbContextTransaction transaction)
        : IOrderedRowLockScope
    {
        private RowLockResource _highest = RowLockResource.None;
        private bool _committed;

        public Task LockMembershipAsync(
            Guid tenantId,
            Guid membershipId,
            CancellationToken cancellationToken = default)
        {
            EnsureCanAcquire(RowLockResource.Membership);
            return db.Database.ExecuteSqlInterpolatedAsync(
                $"""
                SELECT 1 FROM "Memberships"
                WHERE "TenantId" = {tenantId} AND "Id" = {membershipId}
                FOR UPDATE
                """,
                cancellationToken);
        }

        public Task LockMemberAsync(
            Guid tenantId,
            Guid memberId,
            CancellationToken cancellationToken = default)
        {
            EnsureCanAcquire(RowLockResource.Member);
            return db.Database.ExecuteSqlInterpolatedAsync(
                $"""
                SELECT 1 FROM "Members"
                WHERE "TenantId" = {tenantId} AND "Id" = {memberId}
                FOR UPDATE
                """,
                cancellationToken);
        }

        public Task LockCoachAsync(
            Guid tenantId,
            Guid coachStaffId,
            CancellationToken cancellationToken = default)
        {
            EnsureCanAcquire(RowLockResource.Coach);
            return db.Database.ExecuteSqlInterpolatedAsync(
                $"""
                SELECT 1 FROM "Staff"
                WHERE "TenantId" = {tenantId}
                  AND "Id" = {coachStaffId}
                  AND "DeletedAt" IS NULL
                FOR UPDATE
                """,
                cancellationToken);
        }

        public Task LockVisitAsync(
            Guid tenantId,
            Guid visitId,
            CancellationToken cancellationToken = default)
        {
            EnsureCanAcquire(RowLockResource.Visit);
            return db.Database.ExecuteSqlInterpolatedAsync(
                $"""
                SELECT 1 FROM "Visits"
                WHERE "TenantId" = {tenantId} AND "Id" = {visitId}
                FOR UPDATE
                """,
                cancellationToken);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await transaction.CommitAsync(cancellationToken);
            _committed = true;
        }

        public async ValueTask DisposeAsync()
        {
            if (!_committed)
                await transaction.RollbackAsync();

            await transaction.DisposeAsync();
        }

        private void EnsureCanAcquire(RowLockResource resource)
        {
            if (resource <= _highest)
            {
                throw new InvalidOperationException(
                    $"Row lock order violated: cannot acquire {resource} after {_highest}. "
                    + "Global order is Membership → Member → Coach → Visit "
                    + "(see docs/architecture/row-lock-ordering.md).");
            }

            _highest = resource;
        }
    }
}
