namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;

/// <summary>
///     Read contract of Operations and Monitoring for other bounded contexts. It returns immutable projections and performs no user authorization: callers must authorize the ranch or animal before reading. Periods are half-open: from included, to excluded.
/// </summary>
public interface IOperationsMonitoringContextFacade
{
    /// <summary>
    ///     Only readings attributed to an animal are returned, so device-only readings never reach herd indicators.
    /// </summary>
    Task<IReadOnlyList<MonitoringReadingRecord>> GetReadingsAsync(Guid ranchId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns the alerts raised before the period end that were still unacknowledged at the period start or later.
    /// </summary>
    Task<IReadOnlyList<MonitoringAlertRecord>> GetAlertsAsync(Guid ranchId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);

    Task<DeviceCommunicationRecord?> GetLastDeviceCommunicationAsync(Guid deviceId, CancellationToken cancellationToken);

    Task<IReadOnlyList<LivestockOperationRecord>> GetLivestockOperationsAsync(Guid cattleId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);
}
