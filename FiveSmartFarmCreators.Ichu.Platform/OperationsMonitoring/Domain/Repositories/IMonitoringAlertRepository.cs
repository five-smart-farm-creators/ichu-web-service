using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

/// <summary>
///     Domain repository interface for managing <see cref="MonitoringAlert"/> aggregates.
/// </summary>
public interface IMonitoringAlertRepository
{
    /// <summary>
    ///     Adds a new alert asynchronously to the repository.
    /// </summary>
    /// <param name="alert">The alert to add.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    Task AddAsync(MonitoringAlert alert, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds an alert by its identifier asynchronously.
    /// </summary>
    /// <param name="alertId">The alert identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the alert if found; otherwise, <see langword="null"/>.</returns>
    Task<MonitoringAlert?> FindByIdAsync(Guid alertId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the alert that recorded an operation, such as new evidence, asynchronously.
    /// </summary>
    /// <param name="operationId">The operation identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the alert if found; otherwise, <see langword="null"/>.</returns>
    Task<MonitoringAlert?> FindByOperationIdAsync(Guid operationId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the pending, not cleared alerts of an animal for one alert type asynchronously.
    /// </summary>
    /// <param name="cattleId">The animal the alerts refer to.</param>
    /// <param name="type">The alert type.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the open alerts ordered by event time.</returns>
    Task<IReadOnlyList<MonitoringAlert>> FindPendingByCattleAndTypeAsync(Guid cattleId, AlertType type, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds a page of the alerts of a ranch, newest event first, asynchronously.
    /// </summary>
    /// <param name="ranchId">The ranch that owns the alerts.</param>
    /// <param name="status">Only alerts with this status, when given.</param>
    /// <param name="cattleId">Only alerts of this animal, when given.</param>
    /// <param name="skip">The number of alerts to skip.</param>
    /// <param name="take">The maximum number of alerts to return.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the requested alerts.</returns>
    Task<IReadOnlyList<MonitoringAlert>> FindByRanchAsync(Guid ranchId, AlertStatus? status, Guid? cattleId, int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Counts the alerts of a ranch that match the filters asynchronously.
    /// </summary>
    /// <param name="ranchId">The ranch that owns the alerts.</param>
    /// <param name="status">Only alerts with this status, when given.</param>
    /// <param name="cattleId">Only alerts of this animal, when given.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the number of matching alerts.</returns>
    Task<int> CountByRanchAsync(Guid ranchId, AlertStatus? status, Guid? cattleId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the alerts raised before the period end that were not yet acknowledged when the period started asynchronously.
    /// </summary>
    /// <param name="ranchId">The ranch that owns the alerts.</param>
    /// <param name="period">The half-open period.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the alerts ordered by event time.</returns>
    Task<IReadOnlyList<MonitoringAlert>> FindActiveDuringAsync(Guid ranchId, TimePeriod period, CancellationToken cancellationToken = default);
}
