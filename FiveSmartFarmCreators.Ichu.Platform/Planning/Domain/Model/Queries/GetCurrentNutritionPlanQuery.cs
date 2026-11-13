namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Queries;

/// <summary>
/// Finds the plan in force for a lot on a day. A null day stands for the current UTC date.
/// </summary>
public sealed record GetCurrentNutritionPlanQuery(Guid LotId, DateOnly? On);
