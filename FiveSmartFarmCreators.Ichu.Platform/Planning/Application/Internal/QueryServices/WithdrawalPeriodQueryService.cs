using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.QueryServices;

/// <summary>
/// Reads withdrawal periods of a ranch, optionally narrowed to one animal that the ranch holds.
/// </summary>
public sealed class WithdrawalPeriodQueryService(
    IWithdrawalPeriodRepository withdrawalPeriods,
    PlanningGuard guard) : IWithdrawalPeriodQueryService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<WithdrawalPeriod>>> HandleAsync(GetWithdrawalPeriodsQuery query, CancellationToken cancellationToken)
    {
        var access = await guard.EnsureRanchAccessAsync(query.RanchId, PlanningPermission.ReadPlanning, cancellationToken);
        if (access.IsFailure)
        {
            return Result<IReadOnlyList<WithdrawalPeriod>>.Failure(access.DomainError);
        }

        if (query.CattleId is { } cattleId)
        {
            var cattle = await guard.EnsureCattleBelongsToRanchAsync(query.RanchId, cattleId, cancellationToken);
            if (cattle.IsFailure)
            {
                return Result<IReadOnlyList<WithdrawalPeriod>>.Failure(cattle.DomainError);
            }
        }

        return Result<IReadOnlyList<WithdrawalPeriod>>.Success(
            await withdrawalPeriods.FindByRanchAsNoTrackingAsync(query.RanchId, query.CattleId, cancellationToken));
    }
}
