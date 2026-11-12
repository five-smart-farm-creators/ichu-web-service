namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

public sealed record NutritionPlanResource(
    Guid Id,
    Guid LotId,
    string Name,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    IReadOnlyList<RationItemResource> Items,
    decimal? TargetDailyGainKilograms,
    string? Notes);
