using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Repository implementation for <see cref="MonitoringThreshold"/> aggregates using Entity Framework Core.
/// </summary>
/// <param name="context">The shared database context.</param>
public sealed class MonitoringThresholdRepository(DbContext context) : IMonitoringThresholdRepository
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
