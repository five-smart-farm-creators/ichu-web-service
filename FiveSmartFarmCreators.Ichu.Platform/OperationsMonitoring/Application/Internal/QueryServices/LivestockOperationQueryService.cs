using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices.ACL;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.QueryServices;

/// <summary>
///     Application query service that implements <see cref="ILivestockOperationQueryService"/> to read the livestock
///     operations of an animal.
/// </summary>
/// <param name="accessControl">The authorization port implemented with the IAM context.</param>
/// <param name="cattleLookup">The anti-corruption port towards Cattle Information.</param>
/// <param name="operations">The livestock operation repository.</param>
public class LivestockOperationQueryService(
    IMonitoringAccessControl accessControl,
    ICattleLookup cattleLookup,
    ILivestockOperationRepository operations) : ILivestockOperationQueryService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<LivestockOperation>, EGetLivestockOperationsError>> Handle(GetLivestockOperationsQuery query, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return new Result<IReadOnlyList<LivestockOperation>, EGetLivestockOperationsError>.Failure(EGetLivestockOperationsError.Unauthenticated);
        }

        var cattle = await cattleLookup.FindAsync(query.AnimalId, cancellationToken);
        if (cattle is null)
        {
            return new Result<IReadOnlyList<LivestockOperation>, EGetLivestockOperationsError>.Failure(EGetLivestockOperationsError.AnimalNotFound);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, cattle.RanchId, MonitoringPermission.ReadMonitoring, cancellationToken))
        {
            return new Result<IReadOnlyList<LivestockOperation>, EGetLivestockOperationsError>.Failure(EGetLivestockOperationsError.Forbidden);
        }

        var history = await operations.FindByCattleAsync(query.AnimalId, query.Period, cancellationToken);
        return new Result<IReadOnlyList<LivestockOperation>, EGetLivestockOperationsError>.Success(history);
    }
}
