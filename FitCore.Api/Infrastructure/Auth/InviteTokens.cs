using System.Security.Cryptography;
using System.Text;

namespace FitCore.Api.Infrastructure.Auth;

public static class InviteTokens
{
    public static string GenerateRaw() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    public static string Hash(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
