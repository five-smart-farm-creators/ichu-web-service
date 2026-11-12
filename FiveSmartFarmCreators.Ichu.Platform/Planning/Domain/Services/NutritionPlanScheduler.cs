using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Services;

/// <summary>
/// Decides whether a new nutrition plan can join the plans of its lot. A ration change ends the open-ended plan that started earlier, and every other overlap is refused.
/// </summary>
public sealed class NutritionPlanScheduler
{
    /// <summary>
    /// Ends the open plan that the candidate replaces. The caller saves these changes in one transaction and discards them when the candidate is refused.
    /// </summary>
    public Result Schedule(NutritionPlan candidate, IReadOnlyList<NutritionPlan> existingPlans)
    {
        foreach (var existing in existingPlans)
        {
            if (existing.IsOpenEnded && existing.ValidFrom < candidate.ValidFrom)
            {
                var closed = existing.CloseBefore(candidate.ValidFrom);
                if (closed.IsFailure)
                {
                    return closed;
                }
            }
            else if (existing.Overlaps(candidate.ValidFrom, candidate.ValidTo))
            {
                return Result.Failure(PlanningErrors.NutritionPlanOverlap);
            }
        }

        return Result.Success();
    }
}
