using FitCore.Api.Domain.Members;
using FitCore.Api.Domain.Memberships;
using FitCore.Api.Domain.Plans;
using FitCore.Api.Domain.Services;
using FitCore.Api.Domain.Staffs;
using FitCore.Api.Domain.Visits;

namespace FitCore.Api.Domain.Tenants;

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
    public ICollection<Visit> Visits { get; set; } = [];
}
