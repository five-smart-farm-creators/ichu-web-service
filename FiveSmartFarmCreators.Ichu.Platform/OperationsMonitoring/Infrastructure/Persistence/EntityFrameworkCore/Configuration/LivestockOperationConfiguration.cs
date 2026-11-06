using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Entity Framework Core mapping of the <see cref="LivestockOperation"/> aggregate. The shared database context
///     applies it through ApplyConfigurationsFromAssembly.
/// </summary>
public class LivestockOperationConfiguration : IEntityTypeConfiguration<LivestockOperation>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LivestockOperation> builder)
    {
        builder.ToTable("LivestockOperations", MonitoringSchema.Name);
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();
        builder.Property(o => o.Type).HasConversion<string>().HasMaxLength(MonitoringSchema.EnumLength);
        builder.Property(o => o.Observation).HasMaxLength(LivestockOperation.MaximumObservationLength);
        builder.Property(o => o.Product).HasMaxLength(LivestockOperation.MaximumProductLength);
        builder.Property(o => o.Dose).HasMaxLength(LivestockOperation.MaximumDoseLength);
        builder.HasIndex(o => new { o.CattleId, o.OccurredAt });
    }
}
