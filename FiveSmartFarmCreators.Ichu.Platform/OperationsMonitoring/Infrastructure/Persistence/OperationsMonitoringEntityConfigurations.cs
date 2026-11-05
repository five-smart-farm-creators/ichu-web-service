using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence;

/// <summary>EF Core mappings for the monitoring schema. Applied by the shared DbContext through ApplyConfigurationsFromAssembly.</summary>
public sealed class OperationsMonitoringEntityConfigurations :
    IEntityTypeConfiguration<TelemetryReading>,
    IEntityTypeConfiguration<MonitoringAlert>,
    IEntityTypeConfiguration<Geofence>,
    IEntityTypeConfiguration<MonitoringThreshold>,
    IEntityTypeConfiguration<MonitoringRuleSet>,
    IEntityTypeConfiguration<LivestockOperation>
{
    public const string Schema = "monitoring";
    private const int EnumLength = 32;

    public void Configure(EntityTypeBuilder<TelemetryReading> builder)
    {
        builder.ToTable("TelemetryReadings", Schema);
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.HasIndex(r => new { r.DeviceId, r.ReadingId }).IsUnique();
        builder.HasIndex(r => new { r.CattleId, r.RecordedAt });
        builder.HasIndex(r => new { r.RanchId, r.RecordedAt });
        builder.ComplexProperty(r => r.BodyTemperature, ConfigureMeasurement);
        builder.ComplexProperty(r => r.ActivityIndex, ConfigureMeasurement);
        builder.ComplexProperty(r => r.RuminationMinutes, ConfigureMeasurement);
        builder.ComplexProperty(r => r.BatteryPercent, ConfigureMeasurement);
        builder.ComplexProperty(r => r.Position, position =>
        {
            position.Property(p => p.Latitude);
            position.Property(p => p.Longitude);
            position.Property(p => p.AccuracyMeters);
            position.Property(p => p.InvalidReason).HasMaxLength(Measurement.MaximumReasonLength);
        });
    }

    public void Configure(EntityTypeBuilder<MonitoringAlert> builder)
    {
        builder.ToTable("MonitoringAlerts", Schema);
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();
        builder.Property(a => a.Type).HasConversion<string>().HasMaxLength(EnumLength);
        builder.Property(a => a.Severity).HasConversion<string>().HasMaxLength(EnumLength);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(EnumLength);
        builder.Property(a => a.Finding).HasMaxLength(MonitoringAlert.MaximumTextLength);
        builder.Property(a => a.Action).HasMaxLength(MonitoringAlert.MaximumTextLength);
        builder.Property(a => a.Version).IsConcurrencyToken();
        builder.Ignore(a => a.Threshold);
        builder.Ignore(a => a.ResponseTime);
        builder.Ignore(a => a.ExitDuration);
        builder.HasIndex(a => new { a.RanchId, a.Status, a.RaisedAt });
        builder.HasIndex(a => new { a.CattleId, a.Type, a.Status });

        builder.OwnsMany(a => a.Evidence, evidence =>
        {
            evidence.ToTable("AlertEvidence", Schema);
            evidence.WithOwner().HasForeignKey("AlertId");
            evidence.HasKey("AlertId", "DeviceId", "ReadingId");
        });
        builder.Navigation(a => a.Evidence).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(a => a.Changes, change =>
        {
            change.ToTable("AlertChanges", Schema);
            change.WithOwner().HasForeignKey("AlertId");
            change.HasKey(c => c.OperationId);
            change.Property(c => c.OperationId).ValueGeneratedNever();
            change.Property(c => c.Kind).HasConversion<string>().HasMaxLength(EnumLength);
            change.Property(c => c.Fingerprint).HasMaxLength(64).IsRequired();
        });
        builder.Navigation(a => a.Changes).UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    public void Configure(EntityTypeBuilder<Geofence> builder)
    {
        builder.ToTable("Geofences", Schema);
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).ValueGeneratedNever();
        builder.Property(g => g.Name).HasMaxLength(Geofence.MaximumNameLength).IsRequired();
        builder.Property(g => g.Severity).HasConversion<string>().HasMaxLength(EnumLength);
        builder.HasIndex(g => new { g.RanchId, g.LotId }).IsUnique();

        // Vertices use a surrogate key because a redefinition replaces the whole boundary.
        builder.OwnsMany(g => g.Vertices, vertex =>
        {
            vertex.ToTable("GeofenceVertices", Schema);
            vertex.WithOwner().HasForeignKey("GeofenceId");
            vertex.Property<long>("Id").ValueGeneratedOnAdd();
            vertex.HasKey("Id");
            vertex.HasIndex("GeofenceId", nameof(Domain.Model.Entities.GeofenceVertex.Sequence)).IsUnique();
        });
        builder.Navigation(g => g.Vertices).UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    public void Configure(EntityTypeBuilder<MonitoringThreshold> builder)
    {
        builder.ToTable("MonitoringThresholds", Schema);
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.LifeStage).HasMaxLength(MonitoringThreshold.MaximumLifeStageLength).IsRequired();
        builder.Property(t => t.Metric).HasConversion<string>().HasMaxLength(EnumLength);
        builder.Property(t => t.Severity).HasConversion<string>().HasMaxLength(EnumLength);
        builder.ComplexProperty(t => t.Range, range =>
        {
            range.Property(r => r.MinValue);
            range.Property(r => r.MaxValue);
        });
        builder.HasIndex(t => new { t.RanchId, t.LifeStage, t.Metric }).IsUnique();
    }

    public void Configure(EntityTypeBuilder<MonitoringRuleSet> builder)
    {
        builder.ToTable("MonitoringRuleSets", Schema);
        builder.HasKey(r => r.RanchId);
        builder.Property(r => r.RanchId).ValueGeneratedNever();
        builder.Property(r => r.Revision).IsConcurrencyToken();
    }

    public void Configure(EntityTypeBuilder<LivestockOperation> builder)
    {
        builder.ToTable("LivestockOperations", Schema);
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();
        builder.Property(o => o.Type).HasConversion<string>().HasMaxLength(EnumLength);
        builder.Property(o => o.Observation).HasMaxLength(LivestockOperation.MaximumObservationLength);
        builder.Property(o => o.Product).HasMaxLength(LivestockOperation.MaximumProductLength);
        builder.Property(o => o.Dose).HasMaxLength(LivestockOperation.MaximumDoseLength);
        builder.HasIndex(o => new { o.CattleId, o.OccurredAt });
    }

    private static void ConfigureMeasurement(ComplexPropertyBuilder<Measurement> measurement)
    {
        measurement.Property(m => m.Value);
        measurement.Property(m => m.InvalidReason).HasMaxLength(Measurement.MaximumReasonLength);
    }
}
