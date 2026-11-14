using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

/// <summary>
///     Stores the thresholds configured by ranches.
/// </summary>
public interface IMonitoringThresholdRepository
{
    Task AddAsync(MonitoringThreshold threshold, CancellationToken cancellationToken = default);

    Task<MonitoringThreshold?> FindByRanchLifeStageAndMetricAndSeverityAsync(
        Guid ranchId,
        string lifeStage,
        MonitoredMetric metric,
        AlertSeverity severity,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the thresholds of a ranch for one life stage and metric, at most one per severity.
    /// </summary>
    Task<IReadOnlyList<MonitoringThreshold>> FindByRanchLifeStageAndMetricAsync(Guid ranchId, string lifeStage, MonitoredMetric metric, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the thresholds of a ranch ordered by life stage, metric and severity.
    /// </summary>
    Task<IReadOnlyList<MonitoringThreshold>> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken = default);
}
