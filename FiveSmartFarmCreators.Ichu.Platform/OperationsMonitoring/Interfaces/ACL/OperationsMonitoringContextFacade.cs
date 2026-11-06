using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.ACL;

/// <summary>
///     Implements <see cref="IOperationsMonitoringContextFacade"/> over the monitoring repositories.
/// </summary>
/// <param name="readings">The telemetry reading repository.</param>
/// <param name="alerts">The monitoring alert repository.</param>
/// <param name="operations">The livestock operation repository.</param>
public class OperationsMonitoringContextFacade(
    ITelemetryReadingRepository readings,
    IMonitoringAlertRepository alerts,
    ILivestockOperationRepository operations) : IOperationsMonitoringContextFacade
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<MonitoringReadingRecord>> GetReadingsAsync(
        Guid ranchId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var attributed = await readings.FindAttributedByRanchAsync(ranchId, new TimePeriod(from, to), cancellationToken);
        return attributed.Select(ToRecord).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MonitoringAlertRecord>> GetAlertsAsync(
        Guid ranchId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var active = await alerts.FindActiveDuringAsync(ranchId, new TimePeriod(from, to), cancellationToken);
        return active
            .Select(a => new MonitoringAlertRecord(a.Id, a.CattleId, a.Type, a.Severity, a.RaisedAt, a.AcknowledgedAt, a.ClearedAt))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<DeviceCommunicationRecord?> GetLastDeviceCommunicationAsync(Guid deviceId, CancellationToken cancellationToken)
    {
        var latest = await readings.FindLatestByDeviceAsync(deviceId, cancellationToken);
        return latest is null
            ? null
            : new DeviceCommunicationRecord(latest.DeviceId, latest.RecordedAt, latest.ReceivedAt, latest.BatteryPercent.Value);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LivestockOperationRecord>> GetLivestockOperationsAsync(
        Guid cattleId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var history = await operations.FindByCattleAsync(cattleId, new TimePeriod(from, to), cancellationToken);
        return history
            .Select(o => new LivestockOperationRecord(o.Id, o.RanchId, o.CattleId, o.Type, LivestockOperation.IsClinical(o.Type),
                o.OccurredAt, o.AuthorId, o.Product, o.Dose, o.Observation))
            .ToList();
    }

    private static MonitoringReadingRecord ToRecord(TelemetryReading reading) =>
        new(
            reading.ReadingId,
            reading.DeviceId,
            reading.CattleId!.Value,
            reading.RecordedAt,
            reading.BodyTemperature.Value,
            reading.ActivityIndex.Value,
            reading.RuminationMinutes.Value,
            reading.Position.Latitude,
            reading.Position.Longitude,
            reading.Position.AccuracyMeters);
}
