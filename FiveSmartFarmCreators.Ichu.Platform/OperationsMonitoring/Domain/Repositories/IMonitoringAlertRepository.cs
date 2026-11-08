using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

/// <summary>
///     Stores monitoring alerts. Lookups that precede a change are tracked; read-only lookups do not track.
/// </summary>
public interface IMonitoringAlertRepository
{
    Task AddAsync(MonitoringAlert alert, CancellationToken cancellationToken = default);

    Task<MonitoringAlert?> FindByIdAsync(MonitoringAlertId alertId, CancellationToken cancellationToken = default);

    Task<MonitoringAlert?> FindByIdAsNoTrackingAsync(MonitoringAlertId alertId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the alert that recorded an operation, such as new evidence.
    /// </summary>
    Task<MonitoringAlert?> FindByOperationIdAsync(Guid operationId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the pending, not cleared alerts of an animal for one alert type, ordered by event time.
    /// </summary>
    Task<IReadOnlyList<MonitoringAlert>> FindPendingByCattleAndTypeAsync(Guid cattleId, AlertType type, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds a page of the alerts of a ranch, newest event first.
    /// </summary>
    Task<IReadOnlyList<MonitoringAlert>> FindByRanchAsync(Guid ranchId, AlertStatus? status, Guid? cattleId, int skip, int take, CancellationToken cancellationToken = default);

    Task<int> CountByRanchAsync(Guid ranchId, AlertStatus? status, Guid? cattleId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the alerts raised before the period end that were not yet acknowledged when the period started, ordered by event time.
    /// </summary>
    Task<IReadOnlyList<MonitoringAlert>> FindActiveDuringAsync(Guid ranchId, TimePeriod period, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Counts the alerts of a ranch that are not cleared, grouped by severity. Severities without open alerts are absent.
    /// </summary>
    Task<IReadOnlyDictionary<AlertSeverity, int>> CountOpenByRanchAndSeverityAsync(Guid ranchId, CancellationToken cancellationToken = default);
}
