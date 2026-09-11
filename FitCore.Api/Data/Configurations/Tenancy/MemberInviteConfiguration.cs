using FitCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitCore.Api.Data.Configurations.Tenancy;

public sealed class MemberInviteConfiguration : IEntityTypeConfiguration<MemberInvite>
{
    public void Configure(EntityTypeBuilder<MemberInvite> entity)
    {
        entity.HasIndex(i => i.TokenHash).IsUnique();

        entity.HasIndex(i => i.MemberId)
            .IsUnique()
            .HasFilter("\"UsedAt\" IS NULL")
            .HasDatabaseName("IX_MemberInvites_OneUnusedPerMember");

        entity.HasOne(i => i.Member)
            .WithMany()
            .HasForeignKey(i => i.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(i => i.Tenant)
            .WithMany()
            .HasForeignKey(i => i.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
