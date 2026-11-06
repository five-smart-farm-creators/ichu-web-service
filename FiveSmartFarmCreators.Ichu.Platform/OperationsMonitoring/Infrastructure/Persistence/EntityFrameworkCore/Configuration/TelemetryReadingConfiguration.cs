using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Entity Framework Core mapping of the <see cref="TelemetryReading"/> aggregate. The shared database context
///     applies it through ApplyConfigurationsFromAssembly.
/// </summary>
public class TelemetryReadingConfiguration : IEntityTypeConfiguration<TelemetryReading>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TelemetryReading> builder)
    {
        builder.ToTable("TelemetryReadings", MonitoringSchema.Name);
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

    private static void ConfigureMeasurement(ComplexPropertyBuilder<Measurement> measurement)
    {
        measurement.Property(m => m.Value);
        measurement.Property(m => m.InvalidReason).HasMaxLength(Measurement.MaximumReasonLength);
    }
}
