using FitCore.Api.Domain.Entities;

namespace FitCore.Api.Data.Stores.OrganizationOwner.MembersStore;

public interface IMemberStore
{
    Task<IReadOnlyList<Member>> ListByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<Tenant?> FindTenantByIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<bool> EmailTakenAsync(
        Guid tenantId,
        string normalizedEmail,
        CancellationToken cancellationToken = default);

    Task<bool> PhoneTakenAsync(
        Guid tenantId,
        string phone,
        CancellationToken cancellationToken = default);

    Task AddAsync(Member member);

    Task<Member?> FindActiveByIdAsync(
        Guid tenantId,
        Guid memberId,
        CancellationToken cancellationToken = default);

    Task<Member?> FindActiveWithTenantAsync(
        Guid tenantId,
        Guid memberId,
        CancellationToken cancellationToken = default);

    Task InvalidateUnusedInvitesAsync(
        Guid tenantId,
        Guid memberId,
        DateTime usedAt,
        CancellationToken cancellationToken = default);

    Task AddInviteAsync(MemberInvite invite);

    Task<MemberInvite?> FindValidInviteByTokenHashAsync(
        string tokenHash,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Member>> FindCandidatesByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
