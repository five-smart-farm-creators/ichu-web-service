using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Transform;

/// <summary>
///     Projects a nutrition plan to the REST contract. Its ration items are listed by kind and then by feed, so that the order does not depend on storage.
/// </summary>
public static class NutritionPlanResourceFromEntityAssembler
{
    public static NutritionPlanResource ToResourceFromEntity(NutritionPlan plan) =>
        new(
            plan.Id.Value,
            plan.LotId,
            plan.Name,
            plan.ValidFrom,
            plan.ValidTo,
            plan.Items
                .OrderBy(item => item.Kind)
                .ThenBy(item => item.Feed)
                .Select(item => new RationItemResource(item.Feed, item.Kind, item.KilogramsPerHeadPerDay))
                .ToList(),
            plan.TargetDailyGainKilograms,
            plan.Notes);
}
