using FitCore.Api.Domain.Entities;

namespace FitCore.Api.Data.Stores.Platform;

public interface IPlatformAuthStore
{
    Task<PlatformAdmin?> FindAdminByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default);

    Task InvalidateUnusedLoginTokensAsync(
        Guid platformAdminId,
        DateTime usedAt,
        CancellationToken cancellationToken = default);

    Task AddLoginTokenAsync(
        PlatformLoginToken token,
        CancellationToken cancellationToken = default);

    Task<PlatformLoginToken?> FindValidLoginTokenAsync(
        string tokenHash,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
