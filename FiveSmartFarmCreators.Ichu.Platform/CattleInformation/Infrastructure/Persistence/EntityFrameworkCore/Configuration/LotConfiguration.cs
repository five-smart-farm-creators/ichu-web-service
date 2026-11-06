using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
/// Maps ranch lots with concurrency tokens and field-backed membership collections.
/// </summary>
public sealed class LotConfiguration : IEntityTypeConfiguration<Lot>
{
    public void Configure(EntityTypeBuilder<Lot> builder)
    {
        builder.ToTable("lots", "cattle");
        builder.HasKey(lot => lot.Id);
        builder.HasAlternateKey(lot => new { lot.RanchId, lot.Id }).HasName("ak_lots_ranch_id_id");
        builder.Property(lot => lot.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(lot => lot.RanchId).HasColumnName("ranch_id").IsRequired();
        builder.Property(lot => lot.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
        builder.Property(lot => lot.Criterion).HasColumnName("criterion").HasMaxLength(120);
        builder.Property(lot => lot.Version).HasColumnName("version").IsConcurrencyToken();
        builder.Navigation(lot => lot.Memberships)
            .HasField("_memberships")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
