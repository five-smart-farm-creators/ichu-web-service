using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
/// Maps retained stage periods with client-generated identifiers so new history is tracked as inserts.
/// </summary>
public sealed class StagePeriodConfiguration : IEntityTypeConfiguration<StagePeriod>
{
    public void Configure(EntityTypeBuilder<StagePeriod> builder)
    {
        builder.ToTable("stage_periods", "cattle");
        builder.HasKey(period => period.Id);
        builder.Property(period => period.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(period => period.CattleId).HasColumnName("cattle_id").IsRequired();
        builder.Property(period => period.LifeStage)
            .HasColumnName("life_stage").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(period => period.ValidFrom).HasColumnName("valid_from").HasColumnType("date");
        builder.Property(period => period.ValidTo).HasColumnName("valid_to").HasColumnType("date");
        builder.HasIndex(period => new { period.CattleId, period.ValidFrom });
    }
}
