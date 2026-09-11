using System.Security.Cryptography;
using System.Text;
using FitCore.Api.Data.Stores.Platform;
using FitCore.Api.Domain.Enums;
using FitCore.Api.Domain.Entities;
using FitCore.Api.Infrastructure.App;
using FitCore.Api.Infrastructure.Email;
using Microsoft.Extensions.Options;

namespace FitCore.Api.Features.Platform.Invitations;

public class InvitationService(
    IPlatformInvitationStore invitationStore,
    IEmailSender emailSender,
    IOptions<AppOptions> appOptions)
{
    public async Task CreateInviteAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var now = DateTime.UtcNow;

        await invitationStore.InvalidateUnusedByEmailAsync(normalizedEmail, now, cancellationToken);

        var rawToken = GenerateRawToken();

        await invitationStore.AddAsync(new Invitation
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            TokenHash = HashToken(rawToken),
            Plan = InvitationPlan.Free,
            CreatedAt = now,
            UsedAt = null,
            ExpiresAt = now.AddDays(1),
        }, cancellationToken);

        await invitationStore.SaveChangesAsync(cancellationToken);

        var inviteUrl = ClientLinks.Activate(
            appOptions.Value.ClientBaseUrl,
            "tenant/registry",
            rawToken);

        var html = $"""
            <p>Register your gym with FitCore.</p>
            <p><a href="{inviteUrl}">Click here to continue</a></p>
            <p>This link expires in 24 hours and can be used once.</p>
            """;

        await emailSender.SendAsync(
            normalizedEmail,
            "FitCore organization invite",
            html,
            cancellationToken);
    }

    private static string GenerateRawToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
