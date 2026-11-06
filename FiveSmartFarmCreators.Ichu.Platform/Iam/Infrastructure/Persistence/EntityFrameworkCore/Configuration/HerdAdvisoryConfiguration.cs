using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>Configures the IAM persistence mapping for HerdAdvisory.</summary>
public sealed class HerdAdvisoryConfiguration : IEntityTypeConfiguration<HerdAdvisory>
{
    /// <summary>Applies explicit table, constraint and property mappings.</summary>
    public void Configure(EntityTypeBuilder<HerdAdvisory> builder)
    {
        builder.ToTable("HerdAdvisories", "iam");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(a => a.Scope).HasConversion<string>().HasMaxLength(32);
        builder.Property(a => a.Revision).IsConcurrencyToken();
        builder.HasIndex(a => new { a.VeterinarianId, a.RanchId }).IsUnique()
            .HasFilter("\"Status\" IN ('Pending', 'Granted')");
        builder.HasOne<UserAccount>().WithMany().HasForeignKey(a => a.VeterinarianId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<RanchRegistration>().WithMany().HasForeignKey(a => a.RanchId).OnDelete(DeleteBehavior.Restrict);
    }
}
