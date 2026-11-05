using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Ports;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;

public sealed class TelemetryQueryService(
    MonitoringAuthorization authorization,
    ICattleLookup cattleLookup,
    ITelemetryReadingRepository readings,
    IOptions<OperationsMonitoringOptions> options,
    TimeProvider clock)
{
    public async Task<TelemetryHistory> GetHistoryAsync(GetAnimalTelemetryQuery query, CancellationToken cancellationToken)
    {
        PagingGuard.Ensure(query.Page, query.PageSize, options.Value.MaximumPageSize);
        await AuthorizeAnimalAsync(query.AnimalId, cancellationToken);

        var skip = (query.Page - 1) * query.PageSize;
        var items = await readings.FindByCattleAsync(query.AnimalId, query.Period, skip, query.PageSize, cancellationToken);
        var total = await readings.CountByCattleAsync(query.AnimalId, query.Period, cancellationToken);
        var oldest = await readings.FindOldestRecordedAtAsync(query.AnimalId, cancellationToken);
        return new TelemetryHistory(query.AnimalId, query.Period, oldest, new PagedResult<TelemetryReading>(items, query.Page, query.PageSize, total));
    }

    public async Task<LatestTelemetry> GetLatestAsync(Guid animalId, CancellationToken cancellationToken)
    {
        await AuthorizeAnimalAsync(animalId, cancellationToken);
        return new LatestTelemetry(
            animalId,
            clock.GetUtcNow(),
            await readings.FindLatestAsync(animalId, cancellationToken),
            await readings.FindLatestValidAsync(animalId, TelemetryField.BodyTemperature, cancellationToken),
            await readings.FindLatestValidAsync(animalId, TelemetryField.ActivityIndex, cancellationToken),
            await readings.FindLatestValidAsync(animalId, TelemetryField.RuminationMinutes, cancellationToken),
            await readings.FindLatestValidAsync(animalId, TelemetryField.Position, cancellationToken));
    }

    private async Task AuthorizeAnimalAsync(Guid animalId, CancellationToken cancellationToken)
    {
        var accountId = await authorization.RequireAccountAsync(cancellationToken);
        var cattle = await cattleLookup.FindAsync(animalId, cancellationToken)
            ?? throw MonitoringException.NotFound("The animal does not exist.");
        await authorization.EnsureAsync(accountId, cattle.RanchId, MonitoringPermission.ReadMonitoring, cancellationToken);
    }
}
