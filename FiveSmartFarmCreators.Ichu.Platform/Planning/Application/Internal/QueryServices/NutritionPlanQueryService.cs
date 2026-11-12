using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.QueryServices;

/// <summary>
/// Reads the nutrition plans of a lot for accounts that may read their ranch. The current plan defaults to the UTC date, because ranches have no time zone yet.
/// </summary>
public sealed class NutritionPlanQueryService(
    INutritionPlanRepository plans,
    PlanningGuard guard,
    TimeProvider clock) : INutritionPlanQueryService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<NutritionPlan>>> HandleAsync(GetNutritionPlansByLotQuery query, CancellationToken cancellationToken)
    {
        var access = await guard.EnsureRanchAccessAsync(query.RanchId, PlanningPermission.ReadPlanning, cancellationToken);
        if (access.IsFailure)
        {
            return Result<IReadOnlyList<NutritionPlan>>.Failure(access.DomainError);
        }

        var lot = await guard.EnsureLotBelongsToRanchAsync(query.RanchId, query.LotId, cancellationToken);
        if (lot.IsFailure)
        {
            return Result<IReadOnlyList<NutritionPlan>>.Failure(lot.DomainError);
        }

        return Result<IReadOnlyList<NutritionPlan>>.Success(await plans.FindByLotAsNoTrackingAsync(query.LotId, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<Result<NutritionPlan>> HandleAsync(GetCurrentNutritionPlanQuery query, CancellationToken cancellationToken)
    {
        var access = await guard.EnsureRanchAccessAsync(query.RanchId, PlanningPermission.ReadPlanning, cancellationToken);
        if (access.IsFailure)
        {
            return Result<NutritionPlan>.Failure(access.DomainError);
        }

        var lot = await guard.EnsureLotBelongsToRanchAsync(query.RanchId, query.LotId, cancellationToken);
        if (lot.IsFailure)
        {
            return Result<NutritionPlan>.Failure(lot.DomainError);
        }

        var day = query.On ?? DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        var plan = await plans.FindCurrentByLotAsNoTrackingAsync(query.LotId, day, cancellationToken);
        return plan is null
            ? Result<NutritionPlan>.Failure(PlanningErrors.NoCurrentNutritionPlan)
            : Result<NutritionPlan>.Success(plan);
    }
}
