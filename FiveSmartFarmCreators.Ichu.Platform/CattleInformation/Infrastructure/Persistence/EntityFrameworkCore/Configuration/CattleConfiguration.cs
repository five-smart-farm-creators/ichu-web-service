using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
/// Maps animal identity, ranch-scoped ear-tag uniqueness and optimistic concurrency in the module schema.
/// </summary>
public sealed class CattleConfiguration : IEntityTypeConfiguration<Cattle>
{
    public void Configure(EntityTypeBuilder<Cattle> builder)
    {
        builder.ToTable("cattle", "cattle");
        builder.HasKey(cattle => cattle.Id);
        builder.HasAlternateKey(cattle => new { cattle.RanchId, cattle.Id })
            .HasName("ak_cattle_ranch_id_id");
        builder.HasIndex(cattle => new { cattle.RanchId, cattle.EarTag })
            .IsUnique()
            .HasDatabaseName("uq_cattle_ranch_ear_tag");

        builder.Property(cattle => cattle.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(cattle => cattle.RanchId).HasColumnName("ranch_id").IsRequired();
        builder.Property(cattle => cattle.EarTag)
            .HasColumnName("ear_tag")
            .HasConversion(earTag => earTag.Value, value => new EarTag(value))
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(cattle => cattle.Breed)
            .HasColumnName("breed").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(cattle => cattle.Sex)
            .HasColumnName("sex").HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(cattle => cattle.BirthDate).HasColumnName("birth_date").HasColumnType("date");
        builder.Property(cattle => cattle.Status)
            .HasColumnName("status").HasConversion<string>().HasMaxLength(12).IsRequired();
        builder.Property(cattle => cattle.CurrentStage)
            .HasColumnName("current_stage").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(cattle => cattle.ExitReason)
            .HasColumnName("exit_reason").HasConversion<string>().HasMaxLength(15);
        builder.Property(cattle => cattle.ExitDate).HasColumnName("exit_date").HasColumnType("date");
        builder.Property(cattle => cattle.Version).HasColumnName("version").IsConcurrencyToken();
        builder.Ignore(cattle => cattle.Events);

        builder.HasMany(cattle => cattle.StagePeriods)
            .WithOne()
            .HasForeignKey(period => period.CattleId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(cattle => cattle.StagePeriods)
            .HasField("_stagePeriods")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
