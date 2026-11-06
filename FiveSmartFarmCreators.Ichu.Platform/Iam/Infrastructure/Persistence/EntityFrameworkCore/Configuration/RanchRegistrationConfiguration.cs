using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

public sealed class RanchRegistrationConfiguration : IEntityTypeConfiguration<RanchRegistration>
{
    public void Configure(EntityTypeBuilder<RanchRegistration> builder)
    {
        builder.ToTable("RanchRegistrations", "iam");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).HasMaxLength(160).IsRequired();
        builder.Property(r => r.Region).HasMaxLength(120).IsRequired();
        builder.Property(r => r.District).HasMaxLength(120).IsRequired();
    }
}
