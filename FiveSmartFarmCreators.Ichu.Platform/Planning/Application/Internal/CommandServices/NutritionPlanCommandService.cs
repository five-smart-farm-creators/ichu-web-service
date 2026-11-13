using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.CommandServices;

/// <summary>
/// Registers the nutrition plan of a lot. The overlap check, the change of the previous plan and the save run in one transaction, so two concurrent registrations for a lot cannot both pass the check.
/// </summary>
public sealed class NutritionPlanCommandService(
    INutritionPlanRepository plans,
    IPlanningTransaction transaction,
    NutritionPlanScheduler scheduler,
    PlanningGuard guard) : INutritionPlanCommandService
{
    /// <inheritdoc />
    public async Task<Result<NutritionPlan>> HandleAsync(RegisterNutritionPlanRequest request, CancellationToken cancellationToken)
    {
        var lot = await guard.EnsureLotAccessAsync(request.LotId, PlanningPermission.ManagePlanning, cancellationToken);
        if (lot.IsFailure)
        {
            return Result<NutritionPlan>.Failure(lot.DomainError);
        }

        var command = new RegisterNutritionPlanCommand(
            lot.Value,
            request.LotId,
            request.Name,
            request.ValidFrom,
            request.ValidTo,
            request.Items,
            request.TargetDailyGainKilograms,
            request.Notes);
        var candidate = NutritionPlan.Create(command);
        if (candidate.IsFailure)
        {
            return candidate;
        }

        var plan = candidate.Value!;
        return await transaction.ExecuteAsync(async token =>
        {
            var overlapping = await plans.FindOverlappingByLotAsync(plan.LotId, plan.ValidFrom, plan.ValidTo, token);
            var scheduled = scheduler.Schedule(plan, overlapping);
            if (scheduled.IsFailure)
            {
                return Result<NutritionPlan>.Failure(scheduled.DomainError);
            }

            await plans.AddAsync(plan, token);
            return Result<NutritionPlan>.Success(plan);
        }, cancellationToken);
    }
}
