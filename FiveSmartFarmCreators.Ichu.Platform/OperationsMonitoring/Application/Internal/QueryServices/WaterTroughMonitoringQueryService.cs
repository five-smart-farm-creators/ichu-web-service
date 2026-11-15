using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.QueryServices;

public sealed class WaterTroughMonitoringQueryService(
    IMonitoringAccessControl accessControl,
    IIoTAssetsContextFacade ioTAssets,
    IWaterTroughReadingRepository readings,
    IOptions<OperationsMonitoringOptions> options,
    TimeProvider clock) : IWaterTroughMonitoringQueryService
{
    private static readonly TimeSpan CommunicationTimeout = TimeSpan.FromHours(6);

    public async Task<Result<PagedResult<WaterTroughReading>>> Handle(GetWaterControllerReadingsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.ControllerId == Guid.Empty || query.Page < 1 || query.PageSize < 1
            || query.PageSize > options.Value.MaximumPageSize)
        {
            return Result<PagedResult<WaterTroughReading>>.Failure(OperationsMonitoringErrors.InvalidPage);
        }

        var assignment = await ioTAssets.FindWaterControllerAssignmentAsync(query.ControllerId,
            clock.GetUtcNow(), cancellationToken);
        if (assignment is null || !assignment.IsWaterController)
        {
            return Result<PagedResult<WaterTroughReading>>.Failure(OperationsMonitoringErrors.WaterControllerNotFound);
        }

        var accountId = await accessControl.GetCurrentAccountIdAsync(cancellationToken);
        if (accountId is null)
        {
            return Result<PagedResult<WaterTroughReading>>.Failure(OperationsMonitoringErrors.Unauthenticated);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId.Value, assignment.RanchId,
                MonitoringPermission.ReadMonitoring, cancellationToken))
        {
            // A controller from another ranch must not be distinguishable from an unknown controller.
            return Result<PagedResult<WaterTroughReading>>.Failure(OperationsMonitoringErrors.WaterControllerNotFound);
        }

        var skipLong = (long)(query.Page - 1) * query.PageSize;
        if (skipLong > int.MaxValue)
        {
            return Result<PagedResult<WaterTroughReading>>.Failure(OperationsMonitoringErrors.InvalidPage);
        }

        var skip = (int)skipLong;
        var items = await readings.FindByControllerAsync(assignment.RanchId, query.ControllerId, query.Period,
            skip, query.PageSize, cancellationToken);
        var total = await readings.CountByControllerAsync(assignment.RanchId, query.ControllerId, query.Period,
            cancellationToken);
        return Result<PagedResult<WaterTroughReading>>.Success(
            new PagedResult<WaterTroughReading>(items, query.Page, query.PageSize, total));
    }

    public async Task<Result<IReadOnlyList<WaterTroughStatus>>> Handle(GetWaterTroughStatusesQuery query,
        CancellationToken cancellationToken)
    {
        var authorization = await AuthorizeRanchAsync(query.RanchId, cancellationToken);
        if (authorization.IsFailure)
        {
            return Result<IReadOnlyList<WaterTroughStatus>>.Failure(authorization.DomainError);
        }

        var troughs = await ioTAssets.GetWaterTroughMonitoringSnapshotsAsync(query.RanchId, cancellationToken);
        if (troughs.IsFailure)
        {
            return Result<IReadOnlyList<WaterTroughStatus>>.Failure(troughs.DomainError);
        }

        var evaluatedAt = MonitoringTime.Normalize(clock.GetUtcNow());
        var latestReadings = await readings.FindLatestByRanchAsync(query.RanchId, cancellationToken);
        var latestReceived = await readings.FindLatestReceivedByRanchAsync(query.RanchId, cancellationToken);
        var status = troughs.Value!.Select(trough => BuildStatus(trough,
                latestReadings.Where(reading => reading.WaterTroughId == trough.WaterTroughId
                    && (trough.ControllerId is null || reading.ControllerId == trough.ControllerId)
                    && (trough.ControllerAssignedAt is null || reading.CapturedAt >= trough.ControllerAssignedAt))
                    .OrderByDescending(reading => reading.CapturedAt).ThenByDescending(reading => reading.ReadingId)
                    .FirstOrDefault(),
                latestReceived.Where(reading => reading.WaterTroughId == trough.WaterTroughId
                    && (trough.ControllerId is null || reading.ControllerId == trough.ControllerId)
                    && (trough.ControllerAssignedAt is null || reading.CapturedAt >= trough.ControllerAssignedAt))
                    .OrderByDescending(reading => reading.ReceivedAt).ThenByDescending(reading => reading.ReadingId)
                    .Select(reading => (DateTimeOffset?)reading.ReceivedAt).FirstOrDefault(), evaluatedAt))
            .OrderBy(item => item.Name, StringComparer.Ordinal).ThenBy(item => item.WaterTroughId)
            .ToArray();
        return Result<IReadOnlyList<WaterTroughStatus>>.Success(status);
    }

    public async Task<Result<WaterTroughStatus>> Handle(GetWaterTroughStatusQuery query,
        CancellationToken cancellationToken)
    {
        var authorization = await AuthorizeRanchAsync(query.RanchId, cancellationToken);
        if (authorization.IsFailure)
        {
            return Result<WaterTroughStatus>.Failure(authorization.DomainError);
        }

        var troughs = await ioTAssets.GetWaterTroughMonitoringSnapshotsAsync(query.RanchId, cancellationToken);
        if (troughs.IsFailure)
        {
            return Result<WaterTroughStatus>.Failure(troughs.DomainError);
        }

        var trough = troughs.Value!.SingleOrDefault(item => item.WaterTroughId == query.WaterTroughId);
        if (trough is null)
        {
            return Result<WaterTroughStatus>.Failure(OperationsMonitoringErrors.WaterTroughNotFound);
        }

        var latestReadings = await readings.FindLatestByRanchAsync(query.RanchId, cancellationToken);
        var latestReceived = await readings.FindLatestReceivedByRanchAsync(query.RanchId, cancellationToken);
        var latest = latestReadings.Where(reading => reading.WaterTroughId == trough.WaterTroughId
                && (trough.ControllerId is null || reading.ControllerId == trough.ControllerId)
                && (trough.ControllerAssignedAt is null || reading.CapturedAt >= trough.ControllerAssignedAt))
            .OrderByDescending(reading => reading.CapturedAt).ThenByDescending(reading => reading.ReadingId)
            .FirstOrDefault();
        var lastCommunicationAt = latestReceived.Where(reading => reading.WaterTroughId == trough.WaterTroughId
                && (trough.ControllerId is null || reading.ControllerId == trough.ControllerId)
                && (trough.ControllerAssignedAt is null || reading.CapturedAt >= trough.ControllerAssignedAt))
            .OrderByDescending(reading => reading.ReceivedAt).ThenByDescending(reading => reading.ReadingId)
            .Select(reading => (DateTimeOffset?)reading.ReceivedAt).FirstOrDefault();
        return Result<WaterTroughStatus>.Success(BuildStatus(trough, latest, lastCommunicationAt,
            MonitoringTime.Normalize(clock.GetUtcNow())));
    }

    private async Task<Result> AuthorizeRanchAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        if (ranchId == Guid.Empty)
        {
            return Result.Failure(OperationsMonitoringErrors.RanchIdentifierRequired);
        }

        var accountId = await accessControl.GetCurrentAccountIdAsync(cancellationToken);
        if (accountId is null)
        {
            return Result.Failure(OperationsMonitoringErrors.Unauthenticated);
        }

        return await accessControl.CanAccessRanchAsync(accountId.Value, ranchId, MonitoringPermission.ReadMonitoring,
            cancellationToken)
            ? Result.Success()
            : Result.Failure(OperationsMonitoringErrors.Forbidden);
    }

    private static WaterTroughStatus BuildStatus(WaterTroughMonitoringSnapshot trough,
        WaterTroughReading? latest, DateTimeOffset? lastCommunicationAt, DateTimeOffset evaluatedAt)
    {
        var controllerStatus = trough.ControllerId is null
            ? WaterControllerStatus.NoController
            : lastCommunicationAt is null
                ? WaterControllerStatus.NoReadings
                : evaluatedAt - lastCommunicationAt > CommunicationTimeout
                    ? WaterControllerStatus.NoCommunication
                    : WaterControllerStatus.Communicating;

        var measurementStatus = latest is null
            ? WaterMeasurementStatus.Unavailable
            : latest.IsValid ? WaterMeasurementStatus.Valid : WaterMeasurementStatus.Invalid;

        var rangeStatus = trough.MinimumCelsius is null || trough.MaximumCelsius is null
            ? WaterTemperatureRangeStatus.NotConfigured
            : latest is null || !latest.IsValid
                ? WaterTemperatureRangeStatus.Unavailable
                : latest.Temperature < trough.MinimumCelsius
                    ? WaterTemperatureRangeStatus.BelowRange
                    : latest.Temperature > trough.MaximumCelsius
                        ? WaterTemperatureRangeStatus.AboveRange
                        : WaterTemperatureRangeStatus.WithinRange;

        return new WaterTroughStatus(trough.WaterTroughId, trough.Name, trough.LotId, trough.MinimumCelsius,
            trough.MaximumCelsius, controllerStatus, trough.ControllerId, trough.ControllerAssignedAt,
            lastCommunicationAt, measurementStatus, rangeStatus, latest, evaluatedAt);
    }
}
