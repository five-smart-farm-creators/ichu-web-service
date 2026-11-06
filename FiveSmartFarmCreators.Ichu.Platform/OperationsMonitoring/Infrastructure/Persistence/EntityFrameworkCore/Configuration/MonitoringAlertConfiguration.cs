using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Entity Framework Core mapping of the <see cref="MonitoringAlert"/> aggregate. The shared database context
///     applies it through ApplyConfigurationsFromAssembly.
/// </summary>
public sealed class MonitoringAlertConfiguration : IEntityTypeConfiguration<MonitoringAlert>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MonitoringAlert> builder)
    {
        builder.ToTable("MonitoringAlerts", MonitoringSchema.Name);
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();
        builder.Property(a => a.Type).HasConversion<string>().HasMaxLength(MonitoringSchema.EnumLength);
        builder.Property(a => a.Severity).HasConversion<string>().HasMaxLength(MonitoringSchema.EnumLength);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(MonitoringSchema.EnumLength);
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
            evidence.ToTable("AlertEvidence", MonitoringSchema.Name);
            evidence.WithOwner().HasForeignKey("AlertId");
            evidence.HasKey("AlertId", "DeviceId", "ReadingId");
        });
        builder.Navigation(a => a.Evidence).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(a => a.Changes, change =>
        {
            change.ToTable("AlertChanges", MonitoringSchema.Name);
            change.WithOwner().HasForeignKey("AlertId");
            change.HasKey(c => c.OperationId);
            change.Property(c => c.OperationId).ValueGeneratedNever();
            change.Property(c => c.Kind).HasConversion<string>().HasMaxLength(MonitoringSchema.EnumLength);
            change.Property(c => c.OperationData).IsRequired();
        });
        builder.Navigation(a => a.Changes).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
