using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.QueryServices;

/// <summary>
///     Reads the reproductive history of an animal for accounts that may read its ranch monitoring. The age is counted from the UTC date, because ranches have no time zone yet.
/// </summary>
public class ReproductiveHistoryQueryService(
    IMonitoringAccessControl accessControl,
    ICattleLookup cattleLookup,
    ILivestockOperationRepository operations,
    ReproductiveHistoryCalculator calculator,
    TimeProvider clock) : IReproductiveHistoryQueryService
{
    /// <inheritdoc />
    public async Task<Result<ReproductiveHistoryReport>> Handle(GetReproductiveHistoryQuery query, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return Result<ReproductiveHistoryReport>.Failure(OperationsMonitoringErrors.Unauthenticated);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, query.RanchId, MonitoringPermission.ReadMonitoring, cancellationToken))
        {
            return Result<ReproductiveHistoryReport>.Failure(OperationsMonitoringErrors.Forbidden);
        }

        var animal = await cattleLookup.FindProfileAsync(query.RanchId, query.AnimalId, cancellationToken);
        if (animal is null)
        {
            return Result<ReproductiveHistoryReport>.Failure(OperationsMonitoringErrors.AnimalNotFound);
        }

        var history = calculator.Calculate(animal.Sex, await operations.FindAllByCattleAsync(animal.CattleId, cancellationToken));
        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        return Result<ReproductiveHistoryReport>.Success(new ReproductiveHistoryReport(animal, AgeInMonths(animal.BirthDate, today), history));
    }

    // The age counts only completed months, so an animal turns one month older on the day of its birth.
    private static int AgeInMonths(DateOnly birthDate, DateOnly today)
    {
        var months = (today.Year - birthDate.Year) * 12 + today.Month - birthDate.Month;
        if (birthDate.AddMonths(months) > today)
        {
            months--;
        }

        return Math.Max(months, 0);
    }
}
