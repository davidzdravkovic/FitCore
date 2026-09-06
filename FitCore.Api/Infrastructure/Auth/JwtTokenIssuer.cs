using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FitCore.Api.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FitCore.Api.Infrastructure.Auth;

public class JwtTokenIssuer(IOptions<JwtOptions> options)
{
    public string CreatePlatformAdminToken(PlatformAdmin admin) =>
        CreateToken(
            subjectId: admin.Id,
            email: admin.Email,
            role: "PlatformAdmin");

    public string CreateTenantOwnerToken(User user) =>
        CreateToken(
            subjectId: user.Id,
            email: user.Email,
            role: "TenantOwner",
            tenantId: user.TenantId);

    private string CreateToken(
        Guid subjectId,
        string email,
        string role,
        Guid? tenantId = null)
    {
        var jwt = options.Value;
        if (string.IsNullOrWhiteSpace(jwt.SigningKey) || jwt.SigningKey.Length < 32)
            throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subjectId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Role, role),
        };

        if (tenantId is Guid tid)
            claims.Add(new Claim("tenant_id", tid.ToString()));

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwt.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
