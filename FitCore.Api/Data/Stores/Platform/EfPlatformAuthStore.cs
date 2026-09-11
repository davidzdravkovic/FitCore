using FitCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitCore.Api.Data.Stores.Platform;

public class EfPlatformAuthStore(AppDbContext db) : IPlatformAuthStore
{
    public Task<PlatformAdmin?> FindAdminByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        return db.PlatformAdmins
            .FirstOrDefaultAsync(a => a.Email == normalizedEmail, cancellationToken);
    }

    public async Task InvalidateUnusedLoginTokensAsync(
        Guid platformAdminId,
        DateTime usedAt,
        CancellationToken cancellationToken = default)
    {
        var unused = await db.PlatformLoginTokens
            .Where(t => t.PlatformAdminId == platformAdminId && t.UsedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in unused)
            token.UsedAt = usedAt;
    }

    public Task AddLoginTokenAsync(
        PlatformLoginToken token,
        CancellationToken cancellationToken = default)
    {
        db.PlatformLoginTokens.Add(token);
        return Task.CompletedTask;
    }

    public Task<PlatformLoginToken?> FindValidLoginTokenAsync(
        string tokenHash,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        return db.PlatformLoginTokens
            .Include(t => t.PlatformAdmin)
            .FirstOrDefaultAsync(
                t => t.TokenHash == tokenHash && t.UsedAt == null && t.ExpiresAt > utcNow,
                cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
