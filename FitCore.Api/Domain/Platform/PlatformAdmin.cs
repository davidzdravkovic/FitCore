namespace FitCore.Api.Domain.Platform;

public class PlatformAdmin
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
}
