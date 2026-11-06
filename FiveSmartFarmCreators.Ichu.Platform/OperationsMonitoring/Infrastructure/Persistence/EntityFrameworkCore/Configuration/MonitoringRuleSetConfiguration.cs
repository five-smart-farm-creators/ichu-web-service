using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Entity Framework Core mapping of the <see cref="MonitoringRuleSet"/> aggregate. The shared database context
///     applies it through ApplyConfigurationsFromAssembly.
/// </summary>
public class MonitoringRuleSetConfiguration : IEntityTypeConfiguration<MonitoringRuleSet>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MonitoringRuleSet> builder)
    {
        builder.ToTable("MonitoringRuleSets", MonitoringSchema.Name);
        builder.HasKey(r => r.RanchId);
        builder.Property(r => r.RanchId).ValueGeneratedNever();
        builder.Property(r => r.Revision).IsConcurrencyToken();
    }
}
