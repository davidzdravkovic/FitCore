using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FitCore.Api.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FitCore.Api.Infrastructure.Auth;

public class JwtTokenIssuer(IOptions<JwtOptions> options)
{
    public string CreatePlatformAdminToken(PlatformAdmin admin)
    {
        var jwt = options.Value;
        if (string.IsNullOrWhiteSpace(jwt.SigningKey) || jwt.SigningKey.Length < 32)
            throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, admin.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, admin.Email),
            new Claim(ClaimTypes.Role, "PlatformAdmin"),
        };

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwt.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
