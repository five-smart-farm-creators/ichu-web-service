using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Entity Framework Core mapping of the <see cref="MonitoringThreshold"/> aggregate. The shared database context
///     applies it through ApplyConfigurationsFromAssembly.
/// </summary>
public sealed class MonitoringThresholdConfiguration : IEntityTypeConfiguration<MonitoringThreshold>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MonitoringThreshold> builder)
    {
        builder.ToTable("MonitoringThresholds", MonitoringSchema.Name);
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.LifeStage).HasMaxLength(MonitoringThreshold.MaximumLifeStageLength).IsRequired();
        builder.Property(t => t.Metric).HasConversion<string>().HasMaxLength(MonitoringSchema.EnumLength);
        builder.Property(t => t.Severity).HasConversion<string>().HasMaxLength(MonitoringSchema.EnumLength);
        builder.ComplexProperty(t => t.Range, range =>
        {
            range.Property(r => r.MinValue);
            range.Property(r => r.MaxValue);
        });
        builder.HasIndex(t => new { t.RanchId, t.LifeStage, t.Metric }).IsUnique();
    }
}
