using FitCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitCore.Api.Data;

public static class PlatformAdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var email = configuration["PlatformAdmin:Email"];
        var password = configuration["PlatformAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var exists = await db.PlatformAdmins
            .AnyAsync(a => a.Email == normalizedEmail, cancellationToken);

        if (exists)
            return;

        db.PlatformAdmins.Add(new PlatformAdmin
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
