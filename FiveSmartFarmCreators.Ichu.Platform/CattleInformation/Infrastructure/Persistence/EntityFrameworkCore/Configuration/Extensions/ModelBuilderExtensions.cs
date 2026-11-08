using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>
///     Entity Framework Core mappings of the Cattle Information context. Ear tags are unique per ranch, and memberships
///     can only reference animals and lots of the same ranch.
/// </summary>
public static class ModelBuilderExtensions
{
    private const string Schema = "cattle";
    private const int EnumLength = 20;

    /// <summary>
    ///     Applies the mappings of animals, lots, memberships and stage periods. The shared database context calls it from OnModelCreating.
    /// </summary>
    public static void ApplyCattleInformationConfiguration(this ModelBuilder builder)
    {
        ConfigureCattle(builder.Entity<Cattle>());
        ConfigureStagePeriod(builder.Entity<StagePeriod>());
        ConfigureLot(builder.Entity<Lot>());
        ConfigureLotMembership(builder.Entity<LotMembership>());
    }

    private static void ConfigureCattle(EntityTypeBuilder<Cattle> cattle)
    {
        cattle.ToTable("cattle", Schema);
        cattle.HasKey(c => c.Id);
        cattle.HasAlternateKey(c => new { c.RanchId, c.Id }).HasName("ak_cattle_ranch_id_id");
        cattle.HasIndex(c => new { c.RanchId, c.EarTag }).IsUnique().HasDatabaseName("uq_cattle_ranch_ear_tag");
        cattle.Property(c => c.Id).HasConversion(CattleIdConverter()).HasColumnName("id").ValueGeneratedNever();
        cattle.Property(c => c.RanchId).HasColumnName("ranch_id").IsRequired();
        cattle.Property(c => c.EarTag)
            .HasConversion(earTag => earTag.Value, value => new EarTag(value))
            .HasColumnName("ear_tag")
            .HasMaxLength(EarTag.MaximumLength)
            .IsRequired();
        cattle.Property(c => c.Breed).HasColumnName("breed").HasConversion<string>().HasMaxLength(30).IsRequired();
        cattle.Property(c => c.Sex).HasColumnName("sex").HasConversion<string>().HasMaxLength(10).IsRequired();
        cattle.Property(c => c.BirthDate).HasColumnName("birth_date").HasColumnType("date");
        cattle.Property(c => c.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(12).IsRequired();
        cattle.Property(c => c.CurrentStage)
            .HasColumnName("current_stage").HasConversion<string>().HasMaxLength(EnumLength).IsRequired();
        cattle.Property(c => c.ExitReason).HasColumnName("exit_reason").HasConversion<string>().HasMaxLength(15);
        cattle.Property(c => c.ExitDate).HasColumnName("exit_date").HasColumnType("date");
        cattle.Property(c => c.Version).HasColumnName("version").IsConcurrencyToken();
        cattle.HasMany(c => c.StagePeriods)
            .WithOne()
            .HasForeignKey(period => period.CattleId)
            .OnDelete(DeleteBehavior.Restrict);
        cattle.Navigation(c => c.StagePeriods).HasField("_stagePeriods").UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureStagePeriod(EntityTypeBuilder<StagePeriod> period)
    {
        period.ToTable("stage_periods", Schema);
        period.HasKey(p => p.Id);
        period.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
        period.Property(p => p.CattleId).HasConversion(CattleIdConverter()).HasColumnName("cattle_id").IsRequired();
        period.Property(p => p.LifeStage)
            .HasColumnName("life_stage").HasConversion<string>().HasMaxLength(EnumLength).IsRequired();
        period.Property(p => p.ValidFrom).HasColumnName("valid_from").HasColumnType("date");
        period.Property(p => p.ValidTo).HasColumnName("valid_to").HasColumnType("date");
        period.HasIndex(p => new { p.CattleId, p.ValidFrom });
    }

    private static void ConfigureLot(EntityTypeBuilder<Lot> lot)
    {
        lot.ToTable("lots", Schema);
        lot.HasKey(l => l.Id);
        lot.HasAlternateKey(l => new { l.RanchId, l.Id }).HasName("ak_lots_ranch_id_id");
        lot.Property(l => l.Id).HasConversion(LotIdConverter()).HasColumnName("id").ValueGeneratedNever();
        lot.Property(l => l.RanchId).HasColumnName("ranch_id").IsRequired();
        lot.Property(l => l.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
        lot.Property(l => l.Criterion).HasColumnName("criterion").HasMaxLength(120);
        lot.Property(l => l.Version).HasColumnName("version").IsConcurrencyToken();
        lot.Navigation(l => l.Memberships).HasField("_memberships").UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureLotMembership(EntityTypeBuilder<LotMembership> membership)
    {
        membership.ToTable("lot_memberships", Schema);
        membership.HasKey(m => new { m.CattleId, m.LotId });
        membership.Property(m => m.CattleId).HasConversion(CattleIdConverter()).HasColumnName("cattle_id");
        membership.Property(m => m.LotId).HasConversion(LotIdConverter()).HasColumnName("lot_id");
        membership.Property(m => m.RanchId).HasColumnName("ranch_id").IsRequired();
        membership.Property(m => m.AssignedAt)
            .HasColumnName("assigned_at").HasColumnType("timestamp with time zone").IsRequired();
        membership.HasOne<Cattle>()
            .WithMany()
            .HasForeignKey(m => new { m.RanchId, m.CattleId })
            .HasPrincipalKey(c => new { c.RanchId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);
        membership.HasOne<Lot>()
            .WithMany(lot => lot.Memberships)
            .HasForeignKey(m => new { m.RanchId, m.LotId })
            .HasPrincipalKey(lot => new { lot.RanchId, lot.Id })
            .OnDelete(DeleteBehavior.ClientCascade);
    }

    private static StronglyTypedIdConverter<CattleId, Guid> CattleIdConverter() =>
        new(id => id.Value, value => new CattleId(value));

    private static StronglyTypedIdConverter<LotId, Guid> LotIdConverter() =>
        new(id => id.Value, value => new LotId(value));
}
