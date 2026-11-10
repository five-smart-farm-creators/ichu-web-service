using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.QueryServices;

/// <summary>
///     Reads the livestock operations of an animal for accounts that may read its monitoring.
/// </summary>
public class LivestockOperationQueryService(
    IMonitoringAccessControl accessControl,
    ICattleLookup cattleLookup,
    ILivestockOperationRepository operations) : ILivestockOperationQueryService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<LivestockOperation>>> Handle(GetLivestockOperationsQuery query, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return Result<IReadOnlyList<LivestockOperation>>.Failure(OperationsMonitoringErrors.Unauthenticated);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, query.RanchId, MonitoringPermission.ReadMonitoring, cancellationToken))
        {
            return Result<IReadOnlyList<LivestockOperation>>.Failure(OperationsMonitoringErrors.Forbidden);
        }

        var cattle = await cattleLookup.FindAsync(query.AnimalId, cancellationToken);
        if (cattle is null || cattle.RanchId != query.RanchId)
        {
            return Result<IReadOnlyList<LivestockOperation>>.Failure(OperationsMonitoringErrors.AnimalNotFound);
        }

        return Result<IReadOnlyList<LivestockOperation>>.Success(
            await operations.FindByCattleAsync(query.AnimalId, query.Period, cancellationToken));
    }
}
