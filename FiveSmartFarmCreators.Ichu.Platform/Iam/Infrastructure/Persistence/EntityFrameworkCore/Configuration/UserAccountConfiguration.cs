using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

public sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.ToTable("UserAccounts", "iam");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.ExternalUid).HasMaxLength(128).IsRequired();
        builder.HasIndex(a => a.ExternalUid).IsUnique();
        builder.Property(a => a.Email).HasConversion(e => e.Value, v => new Email(v)).HasMaxLength(254).IsRequired();
        builder.HasIndex(a => a.Email).IsUnique();
        builder.Property(a => a.Role).HasConversion<string>().HasMaxLength(32);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(a => a.Revision).IsConcurrencyToken();
        builder.OwnsOne(a => a.Profile, profile =>
        {
            profile.ToTable("ProfessionalProfiles", "iam");
            profile.Property(p => p.Specialty).HasMaxLength(120).IsRequired();
            profile.Property(p => p.LicenseNumber).HasMaxLength(80).IsRequired();
            profile.HasIndex(p => p.LicenseNumber).IsUnique();
        });
    }
}
