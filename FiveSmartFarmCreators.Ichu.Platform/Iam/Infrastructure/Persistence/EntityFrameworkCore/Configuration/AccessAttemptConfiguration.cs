using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>Configures the IAM persistence mapping for AccessAttempt.</summary>
public sealed class AccessAttemptConfiguration : IEntityTypeConfiguration<AccessAttempt>
{
    /// <summary>Applies explicit table, constraint and property mappings.</summary>
    public void Configure(EntityTypeBuilder<AccessAttempt> builder)
    {
        builder.ToTable("AccessAttempts", "iam");
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => new { a.UserAccountId, a.RecordedAt });
        builder.HasOne<UserAccount>().WithMany().HasForeignKey(a => a.UserAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}
