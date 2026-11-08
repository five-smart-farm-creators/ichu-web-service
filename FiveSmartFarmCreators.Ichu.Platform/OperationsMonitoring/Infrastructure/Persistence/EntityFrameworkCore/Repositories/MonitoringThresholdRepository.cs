using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Stores the thresholds of ranches with Entity Framework Core.
/// </summary>
public class MonitoringThresholdRepository(AppDbContext context) : IMonitoringThresholdRepository
{
    private DbSet<MonitoringThreshold> Thresholds => context.Set<MonitoringThreshold>();

    /// <inheritdoc />
    public async Task AddAsync(MonitoringThreshold threshold, CancellationToken cancellationToken = default)
    {
        await Thresholds.AddAsync(threshold, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MonitoringThreshold?> FindByRanchLifeStageAndMetricAsync(Guid ranchId, string lifeStage, MonitoredMetric metric, CancellationToken cancellationToken = default) =>
        Thresholds.SingleOrDefaultAsync(t => t.RanchId == ranchId && t.LifeStage == lifeStage && t.Metric == metric, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<MonitoringThreshold>> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken = default) =>
        await Thresholds
            .Where(t => t.RanchId == ranchId)
            .OrderBy(t => t.LifeStage)
            .ThenBy(t => t.Metric)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}
