using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyIoTAssetsConfiguration(this ModelBuilder builder)
    {
        var devices = builder.Entity<IoTDevice>();
        devices.ToTable("devices", "iot", table =>
        {
            table.HasCheckConstraint("ck_iot_devices_type", "device_type IN ('SmartCollar', 'WaterController')");
            table.HasCheckConstraint("ck_iot_devices_status", "status IN ('Available', 'InService', 'Maintenance')");
            table.HasCheckConstraint("ck_iot_devices_battery", "battery_percentage BETWEEN 0 AND 100");
        });
        devices.HasKey(d => d.Id);
        devices.HasAlternateKey(d => new { d.Id, d.RanchId });
        devices.Property(d => d.Id).HasColumnName("id").ValueGeneratedNever();
        devices.Property(d => d.RanchId).HasColumnName("ranch_id").IsRequired();
        devices.Property(d => d.Type).HasColumnName("device_type").HasConversion<string>().HasMaxLength(20).IsRequired();
        devices.Property(d => d.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        devices.Property(d => d.FirmwareVersion).HasColumnName("firmware_version").HasMaxLength(20);
        devices.Property(d => d.LastCommunicationAt).HasColumnName("last_communication_at");
        devices.Property(d => d.LastCommunicationReceivedAt).HasColumnName("last_communication_received_at");
        devices.Property(d => d.BatteryRecordedAt).HasColumnName("battery_recorded_at");
        devices.Property(d => d.Version).HasColumnName("version").IsConcurrencyToken();
        devices.Property(d => d.CreatedAt).HasColumnName("created_at");
        devices.Property(d => d.UpdatedAt).HasColumnName("updated_at");
        devices.Ignore(d => d.DomainEvents);
        devices.OwnsOne(d => d.Serial, serial =>
        {
            serial.Property(s => s.Value).HasColumnName("serial").HasMaxLength(50).IsRequired();
            serial.HasIndex(s => s.Value).IsUnique().HasDatabaseName("ux_iot_devices_serial");
        });
        devices.Navigation(d => d.Serial).IsRequired();
        devices.OwnsOne(d => d.Battery, battery =>
        {
            battery.Property(b => b.Percentage).HasColumnName("battery_percentage").HasColumnType("smallint");
        });
        devices.Navigation(d => d.Battery).IsRequired(false);
        devices.HasMany(d => d.Assignments).WithOne()
            .HasForeignKey(a => new { a.DeviceId, a.RanchId })
            .HasPrincipalKey(d => new { d.Id, d.RanchId })
            .OnDelete(DeleteBehavior.Restrict);
        devices.Navigation(d => d.Assignments).HasField("_assignments").UsePropertyAccessMode(PropertyAccessMode.Field);
        devices.HasIndex(d => d.RanchId);

        var assignments = builder.Entity<DeviceAssignment>();
        assignments.ToTable("device_assignments", "iot", table =>
        {
            table.HasCheckConstraint("ck_iot_assignments_interval", "ended_at IS NULL OR ended_at >= started_at");
            table.HasCheckConstraint("ck_iot_assignments_target", "target_type IN ('Cattle', 'WaterTrough')");
            table.HasCheckConstraint("ck_iot_assignments_release",
                "(ended_at IS NULL AND release_reason IS NULL) OR " +
                "(ended_at IS NOT NULL AND release_reason IS NOT NULL AND length(trim(release_reason)) > 0)");
        });
        assignments.HasKey(a => a.Id);
        assignments.Property(a => a.Id).HasColumnName("id").ValueGeneratedNever();
        assignments.Property(a => a.DeviceId).HasColumnName("device_id");
        assignments.Property(a => a.RanchId).HasColumnName("ranch_id");
        assignments.Property(a => a.TargetId).HasColumnName("target_id");
        assignments.Property(a => a.TargetType).HasColumnName("target_type").HasConversion<string>().HasMaxLength(20);
        assignments.Property(a => a.StartedAt).HasColumnName("started_at");
        assignments.Property(a => a.EndedAt).HasColumnName("ended_at");
        assignments.Property(a => a.ReleaseReason).HasColumnName("release_reason").HasMaxLength(120);
        assignments.HasIndex(a => a.DeviceId).IsUnique()
            .HasFilter("ended_at IS NULL").HasDatabaseName("ux_iot_assignments_open_device");
        assignments.HasIndex(a => new { a.RanchId, a.TargetType, a.TargetId }).IsUnique()
            .HasFilter("ended_at IS NULL").HasDatabaseName("ux_iot_assignments_open_target");

        var troughs = builder.Entity<WaterTrough>();
        troughs.ToTable("water_troughs", "iot", table =>
        {
            table.HasCheckConstraint("ck_iot_troughs_temperature",
                "(min_temperature_celsius IS NULL AND max_temperature_celsius IS NULL) OR " +
                "(min_temperature_celsius IS NOT NULL AND max_temperature_celsius IS NOT NULL " +
                "AND min_temperature_celsius < max_temperature_celsius)");
        });
        troughs.HasKey(t => t.Id);
        troughs.Property(t => t.Id).HasColumnName("id").ValueGeneratedNever();
        troughs.Property(t => t.RanchId).HasColumnName("ranch_id").IsRequired();
        troughs.Property(t => t.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
        troughs.Property(t => t.LotId).HasColumnName("lot_id");
        troughs.Property(t => t.Version).HasColumnName("version").IsConcurrencyToken();
        troughs.Property(t => t.CreatedAt).HasColumnName("created_at");
        troughs.Property(t => t.UpdatedAt).HasColumnName("updated_at");
        troughs.Ignore(t => t.DomainEvents);
        troughs.OwnsOne(t => t.TemperatureRange, range =>
        {
            range.Property(r => r.MinimumCelsius).HasColumnName("min_temperature_celsius").HasPrecision(4, 1);
            range.Property(r => r.MaximumCelsius).HasColumnName("max_temperature_celsius").HasPrecision(4, 1);
        });
        troughs.Navigation(t => t.TemperatureRange).IsRequired(false);
        troughs.HasIndex(t => t.RanchId);
    }
}
