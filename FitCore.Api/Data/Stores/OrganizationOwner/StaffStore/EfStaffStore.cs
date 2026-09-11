using FitCore.Api.Domain.Enums;
using FitCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitCore.Api.Data.Stores.OrganizationOwner.StaffStore;

public class EfStaffStore(AppDbContext db) : IStaffStore
{
    public async Task<IReadOnlyList<Staff>> ListByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.Staff
            .AsNoTracking()
            .Where(s =>
                s.TenantId == tenantId
                && s.Role == StaffRole.Staff
                && s.DeletedAt == null)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync(cancellationToken);
    }

    public Task<Tenant?> FindTenantByIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return db.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
    }

    public Task<bool> EmailTakenAsync(
        Guid tenantId,
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        return db.Staff.AnyAsync(
            s => s.TenantId == tenantId && s.Email == normalizedEmail && s.DeletedAt == null,
            cancellationToken);
    }

    public Task AddAsync(Staff staff)
    {
        db.Staff.Add(staff);
        return Task.CompletedTask;
    }

    public Task<Staff?> FindActiveByIdAsync(
        Guid tenantId,
        Guid staffId,
        CancellationToken cancellationToken = default)
    {
        return db.Staff.FirstOrDefaultAsync(
            s => s.Id == staffId && s.TenantId == tenantId && s.DeletedAt == null,
            cancellationToken);
    }

    public Task<Staff?> FindActiveWithTenantAsync(
        Guid tenantId,
        Guid staffId,
        CancellationToken cancellationToken = default)
    {
        return db.Staff
            .Include(s => s.Tenant)
            .FirstOrDefaultAsync(
                s => s.Id == staffId && s.TenantId == tenantId && s.DeletedAt == null,
                cancellationToken);
    }

    public async Task InvalidateUnusedInvitesAsync(
        Guid tenantId,
        Guid staffId,
        DateTime usedAt,
        CancellationToken cancellationToken = default)
    {
        var unused = await db.StaffInvites
            .Where(i =>
                i.TenantId == tenantId
                && i.StaffId == staffId
                && i.UsedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var invite in unused)
            invite.UsedAt = usedAt;
    }

    public Task AddInviteAsync(StaffInvite invite)
    {
        db.StaffInvites.Add(invite);
        return Task.CompletedTask;
    }

    public Task<StaffInvite?> FindValidInviteByTokenHashAsync(
        string tokenHash,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        return db.StaffInvites
            .Include(i => i.Staff)
            .ThenInclude(s => s.Tenant)
            .FirstOrDefaultAsync(
                i => i.TokenHash == tokenHash && i.UsedAt == null && i.ExpiresAt > utcNow,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Staff>> FindStaffCandidatesByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        return await db.Staff
            .Include(s => s.Tenant)
            .Where(s =>
                s.Email == normalizedEmail
                && s.DeletedAt == null
                && s.Role == StaffRole.Staff)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
