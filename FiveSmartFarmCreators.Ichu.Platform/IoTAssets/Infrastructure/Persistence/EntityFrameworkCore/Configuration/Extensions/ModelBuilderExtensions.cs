using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>
///     Entity Framework Core mappings of the IoT Assets context. Each device has at most one open assignment, and the
///     database enforces that a serial and a target are never held twice.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    ///     Applies the mappings of devices, assignments, water troughs and edge gateways under the iot schema. The shared database context calls it from OnModelCreating.
    /// </summary>
    public static void ApplyIoTAssetsConfiguration(this ModelBuilder builder)
    {
        ConfigureDevice(builder.Entity<IoTDevice>());
        ConfigureDeviceAssignment(builder.Entity<DeviceAssignment>());
        ConfigureWaterTrough(builder.Entity<WaterTrough>());
        ConfigureEdgeGateway(builder.Entity<EdgeGateway>());
    }

    private static void ConfigureDevice(EntityTypeBuilder<IoTDevice> devices)
    {
        devices.ToTable("devices", "iot", table =>
        {
            table.HasCheckConstraint("ck_iot_devices_type", "device_type IN ('SmartCollar', 'WaterController')");
            table.HasCheckConstraint("ck_iot_devices_status", "status IN ('Available', 'InService', 'Maintenance')");
            table.HasCheckConstraint("ck_iot_devices_battery", "battery_percentage BETWEEN 0 AND 100");
        });
        devices.HasKey(d => d.Id);
        devices.HasAlternateKey(d => new { d.Id, d.RanchId });
        devices.Property(d => d.Id).HasConversion(DeviceIdConverter()).HasColumnName("id").ValueGeneratedNever();
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
            serial.Property(s => s.Value).HasColumnName("serial").HasMaxLength(DeviceSerial.MaximumLength).IsRequired();
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
    }

    private static void ConfigureDeviceAssignment(EntityTypeBuilder<DeviceAssignment> assignments)
    {
        assignments.ToTable("device_assignments", "iot", table =>
        {
            table.HasCheckConstraint("ck_iot_assignments_interval", "ended_at IS NULL OR ended_at >= started_at");
            table.HasCheckConstraint("ck_iot_assignments_target", "target_type IN ('Cattle', 'WaterTrough')");
            table.HasCheckConstraint("ck_iot_assignments_release",
                "(ended_at IS NULL AND release_reason IS NULL) OR " +
                "(ended_at IS NOT NULL AND release_reason IS NOT NULL AND length(trim(release_reason)) > 0)");
        });
        assignments.HasKey(a => a.Id);
        assignments.Property(a => a.Id).HasConversion(DeviceAssignmentIdConverter()).HasColumnName("id").ValueGeneratedNever();
        assignments.Property(a => a.DeviceId).HasConversion(DeviceIdConverter()).HasColumnName("device_id");
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
    }

    private static void ConfigureWaterTrough(EntityTypeBuilder<WaterTrough> troughs)
    {
        troughs.ToTable("water_troughs", "iot", table =>
        {
            table.HasCheckConstraint("ck_iot_troughs_temperature",
                "(min_temperature_celsius IS NULL AND max_temperature_celsius IS NULL) OR " +
                "(min_temperature_celsius IS NOT NULL AND max_temperature_celsius IS NOT NULL " +
                "AND min_temperature_celsius < max_temperature_celsius)");
        });
        troughs.HasKey(t => t.Id);
        troughs.Property(t => t.Id).HasConversion(WaterTroughIdConverter()).HasColumnName("id").ValueGeneratedNever();
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

    private static void ConfigureEdgeGateway(EntityTypeBuilder<EdgeGateway> gateways)
    {
        gateways.ToTable("edge_gateways", "iot", table =>
        {
            table.HasCheckConstraint("ck_iot_edge_gateways_status", "status IN ('Active', 'Revoked')");
            table.HasCheckConstraint("ck_iot_edge_gateways_key_hash", "octet_length(key_hash) = 32");
        });
        gateways.HasKey(g => g.Id);
        gateways.Property(g => g.Id).HasConversion(EdgeGatewayIdConverter()).HasColumnName("id").ValueGeneratedNever();
        gateways.Property(g => g.RanchId).HasColumnName("ranch_id").IsRequired();
        gateways.Property(g => g.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
        gateways.Property(g => g.KeyHash)
            .HasConversion(hash => hash.ToBytes(), bytes => new GatewayKeyHash(bytes))
            .HasColumnName("key_hash").HasColumnType("bytea").IsRequired();
        gateways.Property(g => g.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        gateways.Property(g => g.RegisteredAt).HasColumnName("registered_at").IsRequired();
        gateways.Property(g => g.RevokedAt).HasColumnName("revoked_at");
        gateways.Property(g => g.Version).HasColumnName("version").IsConcurrencyToken();
        gateways.Ignore(g => g.DomainEvents);
        gateways.HasIndex(g => g.RanchId);
    }

    private static StronglyTypedIdConverter<DeviceId, Guid> DeviceIdConverter() =>
        new(id => id.Value, value => new DeviceId(value));

    private static StronglyTypedIdConverter<DeviceAssignmentId, Guid> DeviceAssignmentIdConverter() =>
        new(id => id.Value, value => new DeviceAssignmentId(value));

    private static StronglyTypedIdConverter<WaterTroughId, Guid> WaterTroughIdConverter() =>
        new(id => id.Value, value => new WaterTroughId(value));

    private static StronglyTypedIdConverter<EdgeGatewayId, Guid> EdgeGatewayIdConverter() =>
        new(id => id.Value, value => new EdgeGatewayId(value));
}
