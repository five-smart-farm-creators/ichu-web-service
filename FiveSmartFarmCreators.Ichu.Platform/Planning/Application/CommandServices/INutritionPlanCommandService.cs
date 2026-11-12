using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.CommandServices;

/// <summary>
/// Registers nutrition plans after checking the caller's management permission and the lot reference.
/// </summary>
public interface INutritionPlanCommandService
{
    Task<Result<NutritionPlan>> HandleAsync(RegisterNutritionPlanCommand command, CancellationToken cancellationToken);
}
