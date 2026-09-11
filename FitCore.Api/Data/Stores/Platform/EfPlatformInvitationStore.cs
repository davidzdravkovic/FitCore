using FitCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitCore.Api.Data.Stores.Platform;

public class EfPlatformInvitationStore(AppDbContext db) : IPlatformInvitationStore
{
    public async Task InvalidateUnusedByEmailAsync(
        string normalizedEmail,
        DateTime usedAt,
        CancellationToken cancellationToken = default)
    {
        var unused = await db.Invitations
            .Where(i => i.Email == normalizedEmail && i.UsedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var invite in unused)
            invite.UsedAt = usedAt;
    }

    public Task AddAsync(
        Invitation invitation,
        CancellationToken cancellationToken = default)
    {
        db.Invitations.Add(invitation);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
