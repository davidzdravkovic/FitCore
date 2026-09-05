namespace FitCore.Api.Domain.Entities;

public class PlatformLoginToken
{
    public Guid Id { get; set; }
    public Guid PlatformAdminId { get; set; }
    public PlatformAdmin PlatformAdmin { get; set; } = null!;
    public required string TokenHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
