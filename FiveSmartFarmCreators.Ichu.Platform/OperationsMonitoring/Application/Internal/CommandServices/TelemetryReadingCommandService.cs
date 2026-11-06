using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices.ACL;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Services;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.CommandServices;

/// <summary>
///     Application command service that implements <see cref="ITelemetryReadingCommandService"/> to synchronize the
///     readings relayed by Edge Gateways.
/// </summary>
/// <param name="deviceAttributions">The anti-corruption port towards IoT Assets.</param>
/// <param name="cattleLookup">The anti-corruption port towards Cattle Information.</param>
/// <param name="readings">The telemetry reading repository.</param>
/// <param name="unitOfWork">The unit of work used to commit changes.</param>
/// <param name="options">The technical limits of the module.</param>
public sealed class TelemetryReadingCommandService(
    IDeviceAttributionLookup deviceAttributions,
    ICattleLookup cattleLookup,
    ITelemetryReadingRepository readings,
    IUnitOfWork unitOfWork,
    IOptions<OperationsMonitoringOptions> options) : ITelemetryReadingCommandService
{
    /// <inheritdoc />
    public async Task<SyncEntryResult> Handle(SynchronizeReadingCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return await SynchronizeAsync(command, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return EntryResult(command, SyncEntryStatus.Rejected, ex.Message);
        }
        catch (DbUpdateException)
        {
            unitOfWork.DiscardChanges();
            return EntryResult(command, SyncEntryStatus.Conflict, "The reading changed concurrently or already exists; send it again if it was not stored.");
        }
    }

    private async Task<SyncEntryResult> SynchronizeAsync(SynchronizeReadingCommand command, CancellationToken cancellationToken)
    {
        var entry = command.Entry;
        if (entry.RecordedAt is not { } capturedAt)
        {
            return EntryResult(command, SyncEntryStatus.Rejected, "The capture time is required.");
        }

        var recordedAt = capturedAt.ToUniversalTime();
        if (recordedAt > command.ReceivedAt + options.Value.ClockSkewTolerance)
        {
            return EntryResult(command, SyncEntryStatus.Rejected, "The capture time is ahead of the server clock beyond the configured tolerance.");
        }

        var existing = await readings.FindByDeviceAndReadingIdAsync(entry.DeviceId, entry.ReadingId, cancellationToken);
        if (existing is not null)
        {
            var replay = CreateReading(entry, recordedAt, existing.GatewayId, existing.RanchId, existing.CattleId, existing.ReceivedAt);
            return existing.HasSameCaptureAs(replay)
                ? EntryResult(command, SyncEntryStatus.Duplicate, "The reading was already synchronized.")
                : EntryResult(command, SyncEntryStatus.Conflict, "A different reading already uses this identifier.");
        }

        var attribution = await deviceAttributions.FindAsync(entry.DeviceId, recordedAt, cancellationToken);
        if (attribution is null)
        {
            return EntryResult(command, SyncEntryStatus.Rejected, "The device is not registered.");
        }

        if (attribution.RanchId != command.RanchId)
        {
            return EntryResult(command, SyncEntryStatus.Rejected, "The gateway has no access to the ranch of this device.");
        }

        if (attribution.CattleId is { } cattleId)
        {
            var cattle = await cattleLookup.FindAsync(cattleId, cancellationToken);
            if (cattle is null || cattle.RanchId != command.RanchId || !cattle.WasActiveAt(recordedAt))
            {
                return EntryResult(command, SyncEntryStatus.Rejected, "The assigned animal was not active in the herd at the capture time.");
            }
        }

        var reading = CreateReading(entry, recordedAt, command.GatewayId, command.RanchId, attribution.CattleId, command.ReceivedAt);
        await readings.AddAsync(reading, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        var note = attribution.CattleId is null
            ? "No device assignment was effective at the capture time; the reading is kept as device data and excluded from herd indicators."
            : null;
        return EntryResult(command, SyncEntryStatus.Accepted, note);
    }

    private static SyncEntryResult EntryResult(SynchronizeReadingCommand command, SyncEntryStatus status, string? reason) =>
        new(SyncEntryKind.Reading, command.Entry.ReadingId, status, reason);

    private static TelemetryReading CreateReading(
        EdgeReadingEntry entry,
        DateTimeOffset recordedAt,
        Guid gatewayId,
        Guid ranchId,
        Guid? cattleId,
        DateTimeOffset receivedAt) =>
        new(
            entry.ReadingId,
            entry.DeviceId,
            recordedAt,
            ToMeasurement(entry.BodyTemperature),
            ToMeasurement(entry.ActivityIndex),
            ToMeasurement(entry.RuminationMinutes),
            ToMeasurement(entry.BatteryPercent),
            entry.Position is { } position
                ? PositionMeasurement.From(position.Latitude, position.Longitude, position.AccuracyMeters, position.InvalidReason)
                : PositionMeasurement.Absent(),
            gatewayId,
            ranchId,
            cattleId,
            receivedAt);

    private static Measurement ToMeasurement(CapturedMetric? metric) =>
        metric is null ? Measurement.Absent() : Measurement.From(metric.Value, metric.InvalidReason);
}
