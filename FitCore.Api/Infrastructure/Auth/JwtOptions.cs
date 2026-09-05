namespace FitCore.Api.Infrastructure.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "FitCore";
    public string Audience { get; set; } = "FitCore";
    public int ExpiryMinutes { get; set; } = 60;
}
