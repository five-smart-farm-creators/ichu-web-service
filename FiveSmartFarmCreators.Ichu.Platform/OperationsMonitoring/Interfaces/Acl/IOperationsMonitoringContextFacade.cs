namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;

/// <summary>
/// Read contract for other bounded contexts. It returns immutable projections and performs no user authorization:
/// callers must authorize the ranch or animal before reading. Periods are half-open: from included, to excluded.
/// </summary>
public interface IOperationsMonitoringContextFacade
{
    /// <summary>Readings attributed to animals of the ranch. Device-only readings are excluded from herd indicators.</summary>
    Task<IReadOnlyList<MonitoringReadingRecord>> GetReadingsAsync(Guid ranchId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);

    /// <summary>Alerts raised before the period end that were still unacknowledged at the period start or later.</summary>
    Task<IReadOnlyList<MonitoringAlertRecord>> GetAlertsAsync(Guid ranchId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);

    /// <summary>The latest synchronized reading of a device, or null when the device never reported.</summary>
    Task<DeviceCommunicationRecord?> GetLastDeviceCommunicationAsync(Guid deviceId, CancellationToken cancellationToken);

    /// <summary>Field events and clinical interventions of an animal ordered by occurrence time.</summary>
    Task<IReadOnlyList<LivestockOperationRecord>> GetLivestockOperationsAsync(Guid cattleId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);
}
