using FitCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitCore.Api.Data.Configurations.Tenancy;

public sealed class StaffInviteConfiguration : IEntityTypeConfiguration<StaffInvite>
{
    public void Configure(EntityTypeBuilder<StaffInvite> entity)
    {
        entity.HasIndex(i => i.TokenHash).IsUnique();

        entity.HasIndex(i => i.StaffId)
            .IsUnique()
            .HasFilter("\"UsedAt\" IS NULL")
            .HasDatabaseName("IX_StaffInvites_OneUnusedPerStaff");

        entity.HasOne(i => i.Staff)
            .WithMany()
            .HasForeignKey(i => i.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(i => i.Tenant)
            .WithMany()
            .HasForeignKey(i => i.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
