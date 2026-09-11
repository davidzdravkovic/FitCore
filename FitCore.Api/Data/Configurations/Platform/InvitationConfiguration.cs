using FitCore.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitCore.Api.Data.Configurations.Platform;

public sealed class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> entity)
    {
        entity.Property(i => i.Plan).HasConversion<string>();

        entity.HasIndex(i => i.TokenHash).IsUnique();

        entity.HasIndex(i => i.Email)
            .IsUnique()
            .HasFilter("\"UsedAt\" IS NULL")
            .HasDatabaseName("IX_Invitations_OneUnusedPerEmail");
    }
}
