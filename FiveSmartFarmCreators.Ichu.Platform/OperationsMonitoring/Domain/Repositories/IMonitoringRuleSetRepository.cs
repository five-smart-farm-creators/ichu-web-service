using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

public interface IMonitoringRuleSetRepository
{
    Task<MonitoringRuleSet?> FindAsync(Guid ranchId, CancellationToken cancellationToken);

    /// <summary>Reads the revision, thresholds and geofences of a ranch from one consistent database snapshot.</summary>
    Task<MonitoringRuleSnapshot> ReadSnapshotAsync(Guid ranchId, CancellationToken cancellationToken);

    void Add(MonitoringRuleSet ruleSet);
}
