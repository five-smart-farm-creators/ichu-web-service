using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.QueryServices;

/// <summary>
/// Reads the nutrition plans of a lot after checking the caller's read permission and the lot reference.
/// </summary>
public interface INutritionPlanQueryService
{
    Task<Result<IReadOnlyList<NutritionPlan>>> HandleAsync(GetNutritionPlansByLotQuery query, CancellationToken cancellationToken);

    Task<Result<NutritionPlan>> HandleAsync(GetCurrentNutritionPlanQuery query, CancellationToken cancellationToken);
}
