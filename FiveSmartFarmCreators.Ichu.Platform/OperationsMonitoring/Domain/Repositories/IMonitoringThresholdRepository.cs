using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

public interface IMonitoringThresholdRepository
{
    Task<MonitoringThreshold?> FindAsync(Guid ranchId, string lifeStage, MonitoredMetric metric, CancellationToken cancellationToken);

    Task<IReadOnlyList<MonitoringThreshold>> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken);

    void Add(MonitoringThreshold threshold);
}
