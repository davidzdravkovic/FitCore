using FitCore.Api.Domain;

namespace FitCore.Api.Domain.Entities;


public class Staff
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public StaffRole Role { get; set; }
    public string? PasswordHash { get; set; }
    public DateTime? DeletedAt { get; set; }
}
