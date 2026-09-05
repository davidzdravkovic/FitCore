
namespace FitCore.Api.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string BusinessEmail { get; set; }
    public required string Country { get; set; }
    public required string City { get; set; }
    public required string TimeZone { get; set; }
    public DateTime CreatedAt { get; set; }
    public TenantStatus Status { get; set; }

    public ICollection<User> Users { get; set; } = [];
}
