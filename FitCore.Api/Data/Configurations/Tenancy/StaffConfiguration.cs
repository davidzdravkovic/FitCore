using FitCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitCore.Api.Data.Configurations.Tenancy;

public sealed class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> entity)
    {
        // Email uniqueness is per tenant: the same address may be staff at multiple gyms.
        entity.ToTable("Staff");

        entity.Property(s => s.Role).HasConversion<string>();

        entity.HasIndex(s => new { s.TenantId, s.Email })
            .IsUnique()
            .HasFilter("\"DeletedAt\" IS NULL")
            .HasDatabaseName("IX_Staff_TenantId_Email");

        entity.HasOne(s => s.Tenant)
            .WithMany(t => t.Staff)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
