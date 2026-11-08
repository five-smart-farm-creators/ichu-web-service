using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.CommandServices;

/// <summary>
///     Synchronizes the readings relayed by Edge Gateways. A reading is identified by its device and firmware identifier, so a retry with the same capture is a duplicate and a different capture under that identifier is a conflict.
/// </summary>
public class TelemetryReadingCommandService(
    IDeviceAttributionLookup deviceAttributions,
    ICattleLookup cattleLookup,
    ITelemetryReadingRepository readings,
    IOperationsMonitoringUnitOfWork unitOfWork,
    IOptions<OperationsMonitoringOptions> options) : ITelemetryReadingCommandService
{
    private const string AlreadySynchronizedNote = "The reading was already synchronized.";

    private const string UnassignedNote =
        "No device assignment was effective at the capture time; the reading is kept as device data and excluded from herd indicators.";

    /// <inheritdoc />
    public async Task<SyncEntryResult> Handle(SynchronizeReadingCommand command, CancellationToken cancellationToken)
    {
        var entry = command.Entry;
        if (entry.RecordedAt is not { } capturedAt)
        {
            return Rejected(command, OperationsMonitoringErrors.CaptureTimeRequired);
        }

        var recordedAt = MonitoringTime.Normalize(capturedAt);
        if (recordedAt > command.ReceivedAt + options.Value.ClockSkewTolerance)
        {
            return Rejected(command, OperationsMonitoringErrors.CaptureTimeAhead);
        }

        var existing = await readings.FindByDeviceAndReadingIdAsync(entry.DeviceId, entry.ReadingId, cancellationToken);
        if (existing is not null)
        {
            var replay = BuildReading(entry, recordedAt, existing.GatewayId, existing.RanchId, existing.CattleId, existing.ReceivedAt);
            if (replay.IsFailure)
            {
                return Rejected(command, replay.DomainError);
            }

            return existing.HasSameCaptureAs(replay.Value!)
                ? EntryResult(command, SyncEntryStatus.Duplicate, AlreadySynchronizedNote)
                : Conflict(command, OperationsMonitoringErrors.ReadingIdentifierConflict);
        }

        var attribution = await deviceAttributions.FindAsync(entry.DeviceId, recordedAt, cancellationToken);
        if (attribution is null)
        {
            return Rejected(command, OperationsMonitoringErrors.DeviceNotRegistered);
        }

        if (attribution.RanchId != command.RanchId)
        {
            return Rejected(command, OperationsMonitoringErrors.DeviceGatewayAccessDenied);
        }

        if (attribution.CattleId is { } cattleId)
        {
            var cattle = await cattleLookup.FindAsync(cattleId, cancellationToken);
            if (cattle is null || cattle.RanchId != command.RanchId || !cattle.WasActiveAt(recordedAt))
            {
                return Rejected(command, OperationsMonitoringErrors.ReadingAssignedAnimalNotActive);
            }
        }

        var reading = BuildReading(entry, recordedAt, command.GatewayId, command.RanchId, attribution.CattleId, command.ReceivedAt);
        if (reading.IsFailure)
        {
            return Rejected(command, reading.DomainError);
        }

        await readings.AddAsync(reading.Value!, cancellationToken);
        if (!await unitOfWork.TryCompleteAsync(cancellationToken))
        {
            return Conflict(command, OperationsMonitoringErrors.ReadingConcurrentlyStored);
        }

        var note = attribution.CattleId is null ? UnassignedNote : null;
        return EntryResult(command, SyncEntryStatus.Accepted, note);
    }

    private static SyncEntryResult Rejected(SynchronizeReadingCommand command, Error error) =>
        EntryResult(command, SyncEntryStatus.Rejected, error.Message);

    private static SyncEntryResult Conflict(SynchronizeReadingCommand command, Error error) =>
        EntryResult(command, SyncEntryStatus.Conflict, error.Message);

    private static SyncEntryResult EntryResult(SynchronizeReadingCommand command, SyncEntryStatus status, string? reason) =>
        new(SyncEntryKind.Reading, command.Entry.ReadingId, status, reason);

    private static Result<TelemetryReading> BuildReading(
        EdgeReadingEntry entry,
        DateTimeOffset recordedAt,
        Guid gatewayId,
        Guid ranchId,
        Guid? cattleId,
        DateTimeOffset receivedAt)
    {
        var bodyTemperature = ToMeasurement(entry.BodyTemperature);
        if (bodyTemperature.IsFailure)
        {
            return Result<TelemetryReading>.Failure(bodyTemperature.DomainError);
        }

        var activityIndex = ToMeasurement(entry.ActivityIndex);
        if (activityIndex.IsFailure)
        {
            return Result<TelemetryReading>.Failure(activityIndex.DomainError);
        }

        var ruminationMinutes = ToMeasurement(entry.RuminationMinutes);
        if (ruminationMinutes.IsFailure)
        {
            return Result<TelemetryReading>.Failure(ruminationMinutes.DomainError);
        }

        var batteryPercent = ToMeasurement(entry.BatteryPercent);
        if (batteryPercent.IsFailure)
        {
            return Result<TelemetryReading>.Failure(batteryPercent.DomainError);
        }

        var position = entry.Position is { } captured
            ? PositionMeasurement.From(captured.Latitude, captured.Longitude, captured.AccuracyMeters, captured.InvalidReason)
            : Result<PositionMeasurement>.Success(PositionMeasurement.Absent());
        if (position.IsFailure)
        {
            return Result<TelemetryReading>.Failure(position.DomainError);
        }

        return TelemetryReading.Create(entry.ReadingId, entry.DeviceId, recordedAt, bodyTemperature.Value!, activityIndex.Value!,
            ruminationMinutes.Value!, batteryPercent.Value!, position.Value!, gatewayId, ranchId, cattleId, receivedAt);
    }

    private static Result<Measurement> ToMeasurement(CapturedMetric? metric) =>
        metric is null ? Result<Measurement>.Success(Measurement.Absent()) : Measurement.From(metric.Value, metric.InvalidReason);
}
