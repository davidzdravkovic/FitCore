using FitCore.Api.Domain.Entities;

namespace FitCore.Api.Data.Stores.OrganizationOwner.StaffStore;

public interface IStaffStore
{
    Task<IReadOnlyList<Staff>> ListByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<Tenant?> FindTenantByIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<bool> EmailTakenAsync(
        Guid tenantId,
        string normalizedEmail,
        CancellationToken cancellationToken = default);

    Task AddAsync(Staff staff);

    Task<Staff?> FindActiveByIdAsync(
        Guid tenantId,
        Guid staffId,
        CancellationToken cancellationToken = default);

    Task<Staff?> FindActiveWithTenantAsync(
        Guid tenantId,
        Guid staffId,
        CancellationToken cancellationToken = default);

    Task InvalidateUnusedInvitesAsync(
        Guid tenantId,
        Guid staffId,
        DateTime usedAt,
        CancellationToken cancellationToken = default);

    Task AddInviteAsync(StaffInvite invite);

    Task<StaffInvite?> FindValidInviteByTokenHashAsync(
        string tokenHash,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Staff>> FindStaffCandidatesByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
