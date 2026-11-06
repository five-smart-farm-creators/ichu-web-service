using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

/// <summary>
///     Domain repository interface for managing <see cref="MonitoringThreshold"/> aggregates.
/// </summary>
public interface IMonitoringThresholdRepository
{
    /// <summary>
    ///     Adds a new threshold asynchronously to the repository.
    /// </summary>
    /// <param name="threshold">The threshold to add.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    Task AddAsync(MonitoringThreshold threshold, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the threshold of a ranch for one life stage and metric asynchronously.
    /// </summary>
    /// <param name="ranchId">The ranch that configured the threshold.</param>
    /// <param name="lifeStage">The production stage.</param>
    /// <param name="metric">The monitored metric.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the threshold if found; otherwise, <see langword="null"/>.</returns>
    Task<MonitoringThreshold?> FindByRanchLifeStageAndMetricAsync(Guid ranchId, string lifeStage, MonitoredMetric metric, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the thresholds of a ranch ordered by life stage and metric asynchronously.
    /// </summary>
    /// <param name="ranchId">The ranch that configured the thresholds.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the thresholds of the ranch.</returns>
    Task<IReadOnlyList<MonitoringThreshold>> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken = default);
}
