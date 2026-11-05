using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence;

public sealed class MonitoringThresholdRepository(DbContext context) : IMonitoringThresholdRepository
{
    private DbSet<MonitoringThreshold> Thresholds => context.Set<MonitoringThreshold>();

    public Task<MonitoringThreshold?> FindAsync(Guid ranchId, string lifeStage, MonitoredMetric metric, CancellationToken cancellationToken) =>
        Thresholds.SingleOrDefaultAsync(t => t.RanchId == ranchId && t.LifeStage == lifeStage && t.Metric == metric, cancellationToken);

    public async Task<IReadOnlyList<MonitoringThreshold>> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken) =>
        await Thresholds
            .Where(t => t.RanchId == ranchId)
            .OrderBy(t => t.LifeStage)
            .ThenBy(t => t.Metric)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public void Add(MonitoringThreshold threshold) => Thresholds.Add(threshold);
}
