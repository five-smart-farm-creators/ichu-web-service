using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Acl;

/// <summary>
///     Implements the read contract of the context over the monitoring repositories. Other contexts authorize the ranch or animal before they call it.
/// </summary>
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
            .Select(a => new MonitoringAlertRecord(a.Id.Value, a.CattleId, a.Type, a.Severity, a.RaisedAt, a.AcknowledgedAt, a.ClearedAt))
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
        return history.Select(ToOperationRecord).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LivestockOperationRecord>> GetRanchLivestockOperationsAsync(
        Guid ranchId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var history = await operations.FindByRanchAndPeriodAsNoTrackingAsync(ranchId, new TimePeriod(from, to), cancellationToken);
        return history.Select(ToOperationRecord).ToList();
    }

    /// <inheritdoc />
    public async Task<OpenAlertCounts> GetOpenAlertCountsAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        var counts = await alerts.CountOpenByRanchAndSeverityAsync(ranchId, cancellationToken);
        return new OpenAlertCounts(
            counts.GetValueOrDefault(AlertSeverity.Critical),
            counts.GetValueOrDefault(AlertSeverity.Warning),
            counts.GetValueOrDefault(AlertSeverity.Informative));
    }

    /// <inheritdoc />
    public async Task<ClinicalInterventionRecord?> FindClinicalInterventionAsync(Guid operationId, CancellationToken cancellationToken)
    {
        var operation = await operations.FindByIdAsync(new LivestockOperationId(operationId), cancellationToken);
        return operation is null || !LivestockOperation.IsClinical(operation.Type)
            ? null
            : new ClinicalInterventionRecord(
                operation.Id.Value, operation.RanchId, operation.CattleId, operation.Type.ToString(), operation.OccurredAt);
    }

    private static LivestockOperationRecord ToOperationRecord(LivestockOperation operation) =>
        new(operation.Id.Value, operation.RanchId, operation.CattleId, operation.Type, LivestockOperation.IsClinical(operation.Type),
            operation.OccurredAt, operation.AuthorId, operation.Product, operation.Dose, operation.Observation);

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
