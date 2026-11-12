namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

/// <summary>
///     The nutrition plan of a lot. A null end keeps the plan in force after its start, and the target gain is optional.
/// </summary>
public sealed record RegisterNutritionPlanResource(
    string? Name,
    DateOnly? ValidFrom,
    DateOnly? ValidTo,
    IReadOnlyList<RegisterRationItemResource?>? Items,
    decimal? TargetDailyGainKilograms,
    string? Notes);
