using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

/// <summary>
///     Stores the rule revision counter of each ranch.
/// </summary>
public interface IMonitoringRuleSetRepository
{
    Task AddAsync(MonitoringRuleSet ruleSet, CancellationToken cancellationToken = default);

    Task<MonitoringRuleSet?> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken = default);

    Task<MonitoringRuleSet?> FindByRanchAsNoTrackingAsync(Guid ranchId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads the revision, thresholds and geofences of a ranch from one consistent database snapshot. The revision is zero when the ranch never configured rules.
    /// </summary>
    Task<MonitoringRuleSnapshot> ReadSnapshotAsync(Guid ranchId, CancellationToken cancellationToken = default);
}
