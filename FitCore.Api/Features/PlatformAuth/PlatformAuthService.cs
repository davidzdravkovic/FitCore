using System.Security.Cryptography;
using System.Text;
using FitCore.Api.Data;
using FitCore.Api.Domain.Entities;
using FitCore.Api.Infrastructure.App;
using FitCore.Api.Infrastructure.Auth;
using FitCore.Api.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FitCore.Api.Features.PlatformAuth;

public class PlatformAuthService(
    AppDbContext db,
    IEmailSender emailSender,
    JwtTokenIssuer jwtTokenIssuer,
    IOptions<AppOptions> appOptions)
{
    private static readonly TimeSpan LoginTokenLifetime = TimeSpan.FromMinutes(15);

    public async Task<PlatformAdmin?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var admin = await db.PlatformAdmins
            .FirstOrDefaultAsync(a => a.Email == normalizedEmail, cancellationToken);

        if (admin is null || !BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
            return null;

        return admin;
    }

    public async Task SendLoginMagicLinkAsync(
        PlatformAdmin admin,
        CancellationToken cancellationToken = default)
    {
        string rawToken = GenerateRawToken();
        var now = DateTime.UtcNow;

        var unused = await db.PlatformLoginTokens
            .Where(t => t.PlatformAdminId == admin.Id && t.UsedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in unused)
            token.UsedAt = now;

        db.PlatformLoginTokens.Add(new PlatformLoginToken
        {
            Id = Guid.NewGuid(),
            PlatformAdminId = admin.Id,
            TokenHash = HashToken(rawToken),
            ExpiresAt = now.Add(LoginTokenLifetime),
            CreatedAt = now,
        });

        await db.SaveChangesAsync(cancellationToken);


        var flutterBaseUrl = appOptions.Value.ClientBaseUrl.TrimEnd('/');
        var flutterVerifyUrl =
            $"{flutterBaseUrl}/platform/verify?token={Uri.EscapeDataString(rawToken)}";

        var html = $"""
            <p>Sign in to FitCore platform admin.</p>
            <p><a href="{flutterVerifyUrl}">Click here to continue</a></p>
            <p>This link expires in 15 minutes and can be used once.</p>
            """;

        await emailSender.SendAsync(
            admin.Email,
            "FitCore platform sign-in",
            html,
            cancellationToken);
    }

    public async Task<string?> VerifyLoginTokenAsync(
        string rawToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            return null;

        var hash = HashToken(rawToken.Trim());
        var now = DateTime.UtcNow;

        var loginToken = await db.PlatformLoginTokens
            .Include(t => t.PlatformAdmin)
            .FirstOrDefaultAsync(
                t => t.TokenHash == hash && t.UsedAt == null && t.ExpiresAt > now,
                cancellationToken);

        if (loginToken is null)
            return null;

        loginToken.UsedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        return jwtTokenIssuer.CreatePlatformAdminToken(loginToken.PlatformAdmin);
    }

    private static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }

    private static string GenerateRawToken()
    {
       return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        

    }
}
