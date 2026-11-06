namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;

/// <summary>
///     Read contract of Operations and Monitoring for other bounded contexts. It returns immutable projections and
///     performs no user authorization: callers must authorize the ranch or animal before reading. Periods are
///     half-open: from included, to excluded.
/// </summary>
public interface IOperationsMonitoringContextFacade
{
    /// <summary>
    ///     Gets the readings attributed to animals of a ranch. Device-only readings are excluded from herd indicators.
    /// </summary>
    /// <param name="ranchId">The ranch that owns the devices.</param>
    /// <param name="from">The first instant of the period.</param>
    /// <param name="to">The instant that ends the period, excluded from it.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the attributed readings ordered by capture time.</returns>
    Task<IReadOnlyList<MonitoringReadingRecord>> GetReadingsAsync(Guid ranchId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the alerts raised before the period end that were still unacknowledged at the period start or later.
    /// </summary>
    /// <param name="ranchId">The ranch that owns the alerts.</param>
    /// <param name="from">The first instant of the period.</param>
    /// <param name="to">The instant that ends the period, excluded from it.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the alert lifetimes ordered by event time.</returns>
    Task<IReadOnlyList<MonitoringAlertRecord>> GetAlertsAsync(Guid ranchId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the latest synchronized reading of a device.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the communication record, or <see langword="null"/> when the device never reported.</returns>
    Task<DeviceCommunicationRecord?> GetLastDeviceCommunicationAsync(Guid deviceId, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the field events and clinical interventions of an animal ordered by occurrence time.
    /// </summary>
    /// <param name="cattleId">The animal the operations were applied to.</param>
    /// <param name="from">The first instant of the period.</param>
    /// <param name="to">The instant that ends the period, excluded from it.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the livestock operations of the animal.</returns>
    Task<IReadOnlyList<LivestockOperationRecord>> GetLivestockOperationsAsync(Guid cattleId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);
}
