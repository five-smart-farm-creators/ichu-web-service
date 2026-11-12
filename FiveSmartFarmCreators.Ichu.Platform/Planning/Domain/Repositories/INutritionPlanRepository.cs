using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;

public interface INutritionPlanRepository
{
    Task AddAsync(NutritionPlan plan, CancellationToken cancellationToken);

    /// <summary>
    /// Finds the plans of a lot that overlap the period, and its open-ended plan, which a later plan may end. The plans are tracked, so that a closed plan is saved with the new one.
    /// </summary>
    Task<IReadOnlyList<NutritionPlan>> FindOverlappingByLotAsync(Guid lotId, DateOnly validFrom, DateOnly? validTo, CancellationToken cancellationToken);

    Task<IReadOnlyList<NutritionPlan>> FindByLotAsNoTrackingAsync(Guid lotId, CancellationToken cancellationToken);

    Task<NutritionPlan?> FindCurrentByLotAsNoTrackingAsync(Guid lotId, DateOnly on, CancellationToken cancellationToken);
}
