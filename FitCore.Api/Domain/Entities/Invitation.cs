using FitCore.Api.Domain.Enums;

namespace FitCore.Api.Domain.Entities;

public class Invitation
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public InvitationPlan Plan { get; set; }
    public required string TokenHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
