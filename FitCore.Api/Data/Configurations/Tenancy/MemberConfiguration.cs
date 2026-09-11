using FitCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitCore.Api.Data.Configurations.Tenancy;

public sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> entity)
    {
        entity.Property(m => m.Status).HasConversion<string>();

        entity.ToTable(t => t.HasCheckConstraint(
            "CK_Members_EmailOrPhone",
            "(\"Email\" IS NOT NULL AND btrim(\"Email\") <> '') OR (\"Phone\" IS NOT NULL AND btrim(\"Phone\") <> '')"));

        entity.HasOne(m => m.Tenant)
            .WithMany(t => t.Members)
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(m => new { m.TenantId, m.Email })
            .IsUnique()
            .HasFilter("\"Email\" IS NOT NULL AND \"DeletedAt\" IS NULL")
            .HasDatabaseName("IX_Members_TenantId_Email");

        entity.HasIndex(m => new { m.TenantId, m.Phone })
            .IsUnique()
            .HasFilter("\"Phone\" IS NOT NULL AND \"DeletedAt\" IS NULL")
            .HasDatabaseName("IX_Members_TenantId_Phone");
    }
}
