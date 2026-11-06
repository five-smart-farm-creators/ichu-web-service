using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices.ACL;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Services;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.QueryServices;

/// <summary>
///     Application query service that implements <see cref="ITelemetryReadingQueryService"/> to read the telemetry of an animal.
/// </summary>
/// <param name="accessControl">The authorization port implemented with the IAM context.</param>
/// <param name="cattleLookup">The anti-corruption port towards Cattle Information.</param>
/// <param name="readings">The telemetry reading repository.</param>
/// <param name="options">The technical limits of the module.</param>
/// <param name="clock">The clock that provides the evaluation time.</param>
public sealed class TelemetryReadingQueryService(
    IMonitoringAccessControl accessControl,
    ICattleLookup cattleLookup,
    ITelemetryReadingRepository readings,
    IOptions<OperationsMonitoringOptions> options,
    TimeProvider clock) : ITelemetryReadingQueryService
{
    /// <inheritdoc />
    public async Task<Result<TelemetryHistory, EGetAnimalTelemetryError>> Handle(GetAnimalTelemetryQuery query, CancellationToken cancellationToken)
    {
        if (query.Page < 1 || query.PageSize < 1 || query.PageSize > options.Value.MaximumPageSize)
        {
            return new Result<TelemetryHistory, EGetAnimalTelemetryError>.Failure(EGetAnimalTelemetryError.InvalidPage);
        }

        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return new Result<TelemetryHistory, EGetAnimalTelemetryError>.Failure(EGetAnimalTelemetryError.Unauthenticated);
        }

        var cattle = await cattleLookup.FindAsync(query.AnimalId, cancellationToken);
        if (cattle is null)
        {
            return new Result<TelemetryHistory, EGetAnimalTelemetryError>.Failure(EGetAnimalTelemetryError.AnimalNotFound);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, cattle.RanchId, MonitoringPermission.ReadMonitoring, cancellationToken))
        {
            return new Result<TelemetryHistory, EGetAnimalTelemetryError>.Failure(EGetAnimalTelemetryError.Forbidden);
        }

        var skip = (query.Page - 1) * query.PageSize;
        var items = await readings.FindByCattleAsync(query.AnimalId, query.Period, skip, query.PageSize, cancellationToken);
        var total = await readings.CountByCattleAsync(query.AnimalId, query.Period, cancellationToken);
        var oldest = await readings.FindOldestRecordedAtByCattleAsync(query.AnimalId, cancellationToken);
        var page = new PagedResult<TelemetryReading>(items, query.Page, query.PageSize, total);
        return new Result<TelemetryHistory, EGetAnimalTelemetryError>.Success(new TelemetryHistory(query.AnimalId, query.Period, oldest, page));
    }

    /// <inheritdoc />
    public async Task<Result<LatestTelemetry, EGetLatestAnimalTelemetryError>> Handle(GetLatestAnimalTelemetryQuery query, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return new Result<LatestTelemetry, EGetLatestAnimalTelemetryError>.Failure(EGetLatestAnimalTelemetryError.Unauthenticated);
        }

        var cattle = await cattleLookup.FindAsync(query.AnimalId, cancellationToken);
        if (cattle is null)
        {
            return new Result<LatestTelemetry, EGetLatestAnimalTelemetryError>.Failure(EGetLatestAnimalTelemetryError.AnimalNotFound);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, cattle.RanchId, MonitoringPermission.ReadMonitoring, cancellationToken))
        {
            return new Result<LatestTelemetry, EGetLatestAnimalTelemetryError>.Failure(EGetLatestAnimalTelemetryError.Forbidden);
        }

        var latest = new LatestTelemetry(
            query.AnimalId,
            clock.GetUtcNow(),
            await readings.FindLatestByCattleAsync(query.AnimalId, cancellationToken),
            await readings.FindLatestValidByCattleAsync(query.AnimalId, TelemetryField.BodyTemperature, cancellationToken),
            await readings.FindLatestValidByCattleAsync(query.AnimalId, TelemetryField.ActivityIndex, cancellationToken),
            await readings.FindLatestValidByCattleAsync(query.AnimalId, TelemetryField.RuminationMinutes, cancellationToken),
            await readings.FindLatestValidByCattleAsync(query.AnimalId, TelemetryField.Position, cancellationToken));
        return new Result<LatestTelemetry, EGetLatestAnimalTelemetryError>.Success(latest);
    }
}
