using FitCore.Api.Domain.Entities;

namespace FitCore.Api.Data.Stores.Platform;

public interface IPlatformInvitationStore
{
    Task InvalidateUnusedByEmailAsync(
        string normalizedEmail,
        DateTime usedAt,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Invitation invitation,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
