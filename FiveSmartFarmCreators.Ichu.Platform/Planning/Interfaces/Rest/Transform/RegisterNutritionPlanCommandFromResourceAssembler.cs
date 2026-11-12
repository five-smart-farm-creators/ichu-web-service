using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Transform;

/// <summary>
///     Maps the nutrition plan request body to its command. A null ration item becomes an empty draft, which the domain rejects.
/// </summary>
public static class RegisterNutritionPlanCommandFromResourceAssembler
{
    public static RegisterNutritionPlanCommand ToCommandFromResource(Guid ranchId, Guid lotId, RegisterNutritionPlanResource resource) =>
        new(
            ranchId,
            lotId,
            resource.Name,
            resource.ValidFrom,
            resource.ValidTo,
            (resource.Items ?? []).Select(item => new RationItemDraft(item?.Feed, item?.Kind, item?.KilogramsPerHeadPerDay)).ToList(),
            resource.TargetDailyGainKilograms,
            resource.Notes);
}
