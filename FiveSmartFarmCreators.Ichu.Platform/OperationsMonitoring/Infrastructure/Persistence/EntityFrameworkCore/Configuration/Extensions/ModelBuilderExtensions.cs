using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>
///     Entity Framework Core mappings of the Operations and Monitoring context. Typed identifiers map to the same uuid columns as before, so the typed model does not change the schema.
/// </summary>
public static class ModelBuilderExtensions
{
    private const string Schema = "monitoring";
    private const int EnumLength = 32;

    /// <summary>
    ///     Applies the mappings of the monitoring aggregates. The shared database context calls it from OnModelCreating.
    /// </summary>
    public static void ApplyOperationsMonitoringConfiguration(this ModelBuilder builder)
    {
        ConfigureTelemetryReading(builder.Entity<TelemetryReading>());
        ConfigureMonitoringAlert(builder.Entity<MonitoringAlert>());
        ConfigureGeofence(builder.Entity<Geofence>());
        ConfigureMonitoringThreshold(builder.Entity<MonitoringThreshold>());
        ConfigureMonitoringRuleSet(builder.Entity<MonitoringRuleSet>());
        ConfigureLivestockOperation(builder.Entity<LivestockOperation>());
    }

    private static void ConfigureTelemetryReading(EntityTypeBuilder<TelemetryReading> reading)
    {
        reading.ToTable("TelemetryReadings", Schema);
        reading.HasKey(r => r.Id);
        reading.Property(r => r.Id).HasConversion(TelemetryReadingIdConverter()).ValueGeneratedNever();
        reading.HasIndex(r => new { r.DeviceId, r.ReadingId }).IsUnique();
        reading.HasIndex(r => new { r.CattleId, r.RecordedAt });
        reading.HasIndex(r => new { r.RanchId, r.RecordedAt });
        reading.ComplexProperty(r => r.BodyTemperature, ConfigureMeasurement);
        reading.ComplexProperty(r => r.ActivityIndex, ConfigureMeasurement);
        reading.ComplexProperty(r => r.RuminationMinutes, ConfigureMeasurement);
        reading.ComplexProperty(r => r.BatteryPercent, ConfigureMeasurement);
        reading.ComplexProperty(r => r.Position, position =>
        {
            position.Property(p => p.Latitude);
            position.Property(p => p.Longitude);
            position.Property(p => p.AccuracyMeters);
            position.Property(p => p.InvalidReason).HasMaxLength(Measurement.MaximumReasonLength);
        });
    }

    private static void ConfigureMeasurement(ComplexPropertyBuilder<Measurement> measurement)
    {
        measurement.Property(m => m.Value);
        measurement.Property(m => m.InvalidReason).HasMaxLength(Measurement.MaximumReasonLength);
    }

    private static void ConfigureMonitoringAlert(EntityTypeBuilder<MonitoringAlert> alert)
    {
        alert.ToTable("MonitoringAlerts", Schema);
        alert.HasKey(a => a.Id);
        alert.Property(a => a.Id).HasConversion(MonitoringAlertIdConverter()).ValueGeneratedNever();
        alert.Property(a => a.GeofenceId).HasConversion(NullableGeofenceIdConverter());
        alert.Property(a => a.Type).HasConversion<string>().HasMaxLength(EnumLength);
        alert.Property(a => a.Severity).HasConversion<string>().HasMaxLength(EnumLength);
        alert.Property(a => a.Status).HasConversion<string>().HasMaxLength(EnumLength);
        alert.Property(a => a.Finding).HasMaxLength(MonitoringAlert.MaximumTextLength);
        alert.Property(a => a.Action).HasMaxLength(MonitoringAlert.MaximumTextLength);
        alert.Property(a => a.Version).IsConcurrencyToken();
        alert.Ignore(a => a.Threshold);
        alert.Ignore(a => a.ResponseTime);
        alert.Ignore(a => a.ExitDuration);
        alert.HasIndex(a => new { a.RanchId, a.Status, a.RaisedAt });
        alert.HasIndex(a => new { a.CattleId, a.Type, a.Status });

        alert.OwnsMany(a => a.Evidence, evidence =>
        {
            evidence.ToTable("AlertEvidence", Schema);
            evidence.Property<MonitoringAlertId>("AlertId").HasConversion(MonitoringAlertIdConverter());
            evidence.WithOwner().HasForeignKey("AlertId");
            evidence.HasKey("AlertId", "DeviceId", "ReadingId");
        });
        alert.Navigation(a => a.Evidence).UsePropertyAccessMode(PropertyAccessMode.Field);

        alert.OwnsMany(a => a.Changes, change =>
        {
            change.ToTable("AlertChanges", Schema);
            change.Property<MonitoringAlertId>("AlertId").HasConversion(MonitoringAlertIdConverter());
            change.WithOwner().HasForeignKey("AlertId");
            change.HasKey(c => c.OperationId);
            change.Property(c => c.OperationId).ValueGeneratedNever();
            change.Property(c => c.Kind).HasConversion<string>().HasMaxLength(EnumLength);
            change.Property(c => c.OperationData).IsRequired();
        });
        alert.Navigation(a => a.Changes).UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureGeofence(EntityTypeBuilder<Geofence> geofence)
    {
        geofence.ToTable("Geofences", Schema);
        geofence.HasKey(g => g.Id);
        geofence.Property(g => g.Id).HasConversion(GeofenceIdConverter()).ValueGeneratedNever();
        geofence.Property(g => g.Name).HasMaxLength(Geofence.MaximumNameLength).IsRequired();
        geofence.Property(g => g.Severity).HasConversion<string>().HasMaxLength(EnumLength);
        geofence.HasIndex(g => new { g.RanchId, g.LotId }).IsUnique();

        // Vertices use a surrogate key because a redefinition replaces the whole boundary.
        geofence.OwnsMany(g => g.Vertices, vertex =>
        {
            vertex.ToTable("GeofenceVertices", Schema);
            vertex.Property<GeofenceId>("GeofenceId").HasConversion(GeofenceIdConverter());
            vertex.WithOwner().HasForeignKey("GeofenceId");
            vertex.Property<long>("Id").ValueGeneratedOnAdd();
            vertex.HasKey("Id");
            vertex.HasIndex("GeofenceId", nameof(GeofenceVertex.Sequence)).IsUnique();
        });
        geofence.Navigation(g => g.Vertices).UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureMonitoringThreshold(EntityTypeBuilder<MonitoringThreshold> threshold)
    {
        threshold.ToTable("MonitoringThresholds", Schema);
        threshold.HasKey(t => t.Id);
        threshold.Property(t => t.Id).HasConversion(MonitoringThresholdIdConverter()).ValueGeneratedNever();
        threshold.Property(t => t.LifeStage).HasMaxLength(MonitoringThreshold.MaximumLifeStageLength).IsRequired();
        threshold.Property(t => t.Metric).HasConversion<string>().HasMaxLength(EnumLength);
        threshold.Property(t => t.Severity).HasConversion<string>().HasMaxLength(EnumLength);
        threshold.ComplexProperty(t => t.Range, range =>
        {
            range.Property(r => r.MinValue);
            range.Property(r => r.MaxValue);
        });
        threshold.HasIndex(t => new { t.RanchId, t.LifeStage, t.Metric }).IsUnique();
    }

    private static void ConfigureMonitoringRuleSet(EntityTypeBuilder<MonitoringRuleSet> ruleSet)
    {
        ruleSet.ToTable("MonitoringRuleSets", Schema);
        ruleSet.HasKey(r => r.RanchId);
        ruleSet.Property(r => r.RanchId).ValueGeneratedNever();
        ruleSet.Property(r => r.Revision).IsConcurrencyToken();
    }

    private static void ConfigureLivestockOperation(EntityTypeBuilder<LivestockOperation> operation)
    {
        operation.ToTable("LivestockOperations", Schema);
        operation.HasKey(o => o.Id);
        operation.Property(o => o.Id).HasConversion(LivestockOperationIdConverter()).ValueGeneratedNever();
        operation.Property(o => o.Type).HasConversion<string>().HasMaxLength(EnumLength);
        operation.Property(o => o.Observation).HasMaxLength(LivestockOperation.MaximumObservationLength);
        operation.Property(o => o.Product).HasMaxLength(LivestockOperation.MaximumProductLength);
        operation.Property(o => o.Dose).HasMaxLength(LivestockOperation.MaximumDoseLength);
        operation.HasIndex(o => new { o.CattleId, o.OccurredAt });
    }

    private static StronglyTypedIdConverter<TelemetryReadingId, Guid> TelemetryReadingIdConverter() =>
        new(id => id.Value, value => new TelemetryReadingId(value));

    private static StronglyTypedIdConverter<MonitoringAlertId, Guid> MonitoringAlertIdConverter() =>
        new(id => id.Value, value => new MonitoringAlertId(value));

    private static StronglyTypedIdConverter<GeofenceId, Guid> GeofenceIdConverter() =>
        new(id => id.Value, value => new GeofenceId(value));

    private static StronglyTypedIdConverter<MonitoringThresholdId, Guid> MonitoringThresholdIdConverter() =>
        new(id => id.Value, value => new MonitoringThresholdId(value));

    private static StronglyTypedIdConverter<LivestockOperationId, Guid> LivestockOperationIdConverter() =>
        new(id => id.Value, value => new LivestockOperationId(value));

    private static ValueConverter<GeofenceId?, Guid?> NullableGeofenceIdConverter() =>
        new(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? new GeofenceId(value.Value) : (GeofenceId?)null);
}
