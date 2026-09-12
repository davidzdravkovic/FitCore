using System.Security.Cryptography;
using System.Text;
using FitCore.Api.Data.Stores.Platform;
using FitCore.Api.Domain.Entities;
using FitCore.Api.Errors.Business;
using FitCore.Api.Infrastructure.App;
using FitCore.Api.Infrastructure.Auth;
using FitCore.Api.Infrastructure.Email;

namespace FitCore.Api.Features.Platform.Auth;

public class PlatformAuthService(
    IPlatformAuthStore authStore,
    IEmailSender emailSender,
    JwtTokenIssuer jwtTokenIssuer,
    IClientLinks clientLinks)
{
    private static readonly TimeSpan LoginTokenLifetime = TimeSpan.FromMinutes(15);

    public async Task<Result<PlatformAdmin>> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var admin = await authStore.FindAdminByEmailAsync(normalizedEmail, cancellationToken);

        if (admin is null || !BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
            return Result<PlatformAdmin>.Fail(ErrorCodes.InvalidCredentials);

        return Result<PlatformAdmin>.Success(admin);
    }

    public async Task SendLoginMagicLinkAsync(
        PlatformAdmin admin,
        CancellationToken cancellationToken = default)
    {
        string rawToken = GenerateRawToken();
        var now = DateTime.UtcNow;

        await authStore.InvalidateUnusedLoginTokensAsync(admin.Id, now, cancellationToken);

        await authStore.AddLoginTokenAsync(new PlatformLoginToken
        {
            Id = Guid.NewGuid(),
            PlatformAdminId = admin.Id,
            TokenHash = HashToken(rawToken),
            ExpiresAt = now.Add(LoginTokenLifetime),
            CreatedAt = now,
        }, cancellationToken);

        await authStore.SaveChangesAsync(cancellationToken);

        var flutterVerifyUrl = clientLinks.Activate("platform/verify", rawToken);

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

    public async Task<Result<string>> VerifyLoginTokenAsync(
        string rawToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            return Result<string>.Fail(ErrorCodes.SignInLinkInvalidOrExpired);

        var hash = HashToken(rawToken.Trim());
        var now = DateTime.UtcNow;

        var loginToken = await authStore.FindValidLoginTokenAsync(hash, now, cancellationToken);

        if (loginToken is null)
            return Result<string>.Fail(ErrorCodes.SignInLinkInvalidOrExpired);

        loginToken.UsedAt = now;
        await authStore.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(
            jwtTokenIssuer.CreatePlatformAdminToken(loginToken.PlatformAdmin));
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
