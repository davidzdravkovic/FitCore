using FitCore.Api.Domain.Members;
using FitCore.Api.Domain.Memberships;
using Microsoft.EntityFrameworkCore;

namespace FitCore.Api.Data.Stores.OrganizationOwner.MembersStore;

public class EfMemberStore(AppDbContext db) : IMemberStore
{
    public async Task<IReadOnlyList<Member>> ListByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.Members
            .AsNoTracking()
            .Where(m => m.TenantId == tenantId && MemberStatusRules.OnRoster.Contains(m.Status))
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> EmailTakenAsync(
        Guid tenantId,
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        return db.Members.AnyAsync(
            m => m.TenantId == tenantId
                && m.Email == normalizedEmail
                && MemberStatusRules.OnRoster.Contains(m.Status),
            cancellationToken);
    }

    public Task<bool> PhoneTakenAsync(
        Guid tenantId,
        string phone,
        CancellationToken cancellationToken = default)
    {
        return db.Members.AnyAsync(
            m => m.TenantId == tenantId
                && m.Phone == phone
                && MemberStatusRules.OnRoster.Contains(m.Status),
            cancellationToken);
    }

    public Task AddAsync(Member member)
    {
        db.Members.Add(member);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Membership>> ListUnresolvedMembershipsForMemberAsync(
        Guid tenantId,
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        return await db.Memberships
            .Include(m => m.Plan)
            .Where(m =>
                m.TenantId == tenantId
                && m.MemberId == memberId
                && MembershipStatusRules.Unresolved.Contains(m.Status))
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Member?> FindActiveByIdAsync(
        Guid tenantId,
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        return db.Members.FirstOrDefaultAsync(
            m => m.Id == memberId
                && m.TenantId == tenantId
                && MemberStatusRules.OnRoster.Contains(m.Status),
            cancellationToken);
    }

    public Task<Member?> FindActiveWithTenantAsync(
        Guid tenantId,
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        return db.Members
            .Include(m => m.Tenant)
            .FirstOrDefaultAsync(
                m => m.Id == memberId
                    && m.TenantId == tenantId
                    && MemberStatusRules.OnRoster.Contains(m.Status),
                cancellationToken);
    }

    public async Task InvalidateUnusedInvitesAsync(
        Guid tenantId,
        Guid memberId,
        DateTime usedAt,
        CancellationToken cancellationToken = default)
    {
        var unused = await db.MemberInvites
            .Where(i =>
                i.TenantId == tenantId
                && i.MemberId == memberId
                && i.UsedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var invite in unused)
            invite.UsedAt = usedAt;
    }

    public Task AddInviteAsync(MemberInvite invite)
    {
        db.MemberInvites.Add(invite);
        return Task.CompletedTask;
    }

    public Task<MemberInvite?> FindValidInviteByTokenHashAsync(
        string tokenHash,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        return db.MemberInvites
            .Include(i => i.Member)
            .ThenInclude(m => m.Tenant)
            .FirstOrDefaultAsync(
                i => i.TokenHash == tokenHash && i.UsedAt == null && i.ExpiresAt > utcNow,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Member>> FindCandidatesByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        return await db.Members
            .Include(m => m.Tenant)
            .Where(m => m.Email == normalizedEmail && MemberStatusRules.OnRoster.Contains(m.Status))
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
