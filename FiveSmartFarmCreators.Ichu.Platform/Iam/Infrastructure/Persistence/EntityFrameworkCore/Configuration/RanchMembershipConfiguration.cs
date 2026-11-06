using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>Configures the IAM persistence mapping for RanchMembership.</summary>
public sealed class RanchMembershipConfiguration : IEntityTypeConfiguration<RanchMembership>
{
    /// <summary>Applies explicit table, constraint and property mappings.</summary>
    public void Configure(EntityTypeBuilder<RanchMembership> builder)
    {
        builder.ToTable("RanchMemberships", "iam");
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => new { m.UserAccountId, m.RanchId }).IsUnique();
        builder.HasOne<UserAccount>().WithMany().HasForeignKey(m => m.UserAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<RanchRegistration>().WithMany().HasForeignKey(m => m.RanchId).OnDelete(DeleteBehavior.Restrict);
    }
}
