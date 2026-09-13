using FitCore.Api.Domain.Enums;

namespace FitCore.Api.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string BusinessEmail { get; set; }
    public required string Country { get; set; }
    public required string City { get; set; }
    public required string TimeZone { get; set; }

    /// <summary>ISO 4217 currency code for this gym's operating currency (e.g. USD, EUR).</summary>
    public required string Currency { get; set; }

    public DateTime CreatedAt { get; set; }
    public TenantStatus Status { get; set; }

    public ICollection<Staff> Staff { get; set; } = [];
    public ICollection<Member> Members { get; set; } = [];
    public ICollection<Service> Services { get; set; } = [];
    public ICollection<MembershipPlan> MembershipPlans { get; set; } = [];
    public ICollection<Membership> Memberships { get; set; } = [];
}
