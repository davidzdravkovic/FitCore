using System.Security.Cryptography;
using System.Text;
using FitCore.Api.Data;
using FitCore.Api.Domain;
using FitCore.Api.Domain.Entities;
using FitCore.Api.Infrastructure.App;
using FitCore.Api.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FitCore.Api.Features.Invitations;

public class InvitationService(
    AppDbContext db,
    IEmailSender emailSender,
    IOptions<AppOptions> appOptions)
{
    public async Task CreateInviteAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var now = DateTime.UtcNow;

        var unusedInvites = await db.Invitations
            .Where(i => i.Email == normalizedEmail && i.UsedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var invite in unusedInvites)
            invite.UsedAt = now;

        var rawToken = GenerateRawToken();

        db.Invitations.Add(new Invitation
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            TokenHash = HashToken(rawToken),
            Plan = InvitationPlan.Free,
            CreatedAt = now,
            UsedAt = null,
            ExpiresAt = now.AddDays(1),
        });

        await db.SaveChangesAsync(cancellationToken);

        var flutterBaseUrl = appOptions.Value.ClientBaseUrl.TrimEnd('/');
        var inviteUrl =
            $"{flutterBaseUrl}/tenant/registry?token={Uri.EscapeDataString(rawToken)}";

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
