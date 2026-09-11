using FitCore.Api.Domain.Enums;
using FitCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitCore.Api.Data.Stores.OrganizationOwner.AuthStore;

public class EfOrganizationAuthStore(AppDbContext db) : IOrganizationAuthStore
{
    public Task<Invitation?> FindValidInvitationByTokenHashAsync(
        string tokenHash,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        return db.Invitations
            .FirstOrDefaultAsync(
                i => i.TokenHash == tokenHash && i.UsedAt == null && i.ExpiresAt > utcNow,
                cancellationToken);
    }

    public Task MarkInvitationUsedAsync(Invitation invitation, DateTime usedAt)
    {
        invitation.UsedAt = usedAt;
        return Task.CompletedTask;
    }

    public Task AddTenantAsync(Tenant tenant)
    {
        db.Tenants.Add(tenant);
        return Task.CompletedTask;
    }

    public Task AddOwnerAsync(Staff owner)
    {
        db.Staff.Add(owner);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Staff>> FindOwnerCandidatesByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        return await db.Staff
            .Include(s => s.Tenant)
            .Where(s =>
                s.Email == normalizedEmail
                && s.DeletedAt == null
                && s.Role == StaffRole.Owner)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
