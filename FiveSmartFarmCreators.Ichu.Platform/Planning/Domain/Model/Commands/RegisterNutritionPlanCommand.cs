namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;

/// <summary>
/// Registers the nutrition plan of a lot. Missing values stay null, so that the aggregate reports them as validation errors.
/// </summary>
public sealed record RegisterNutritionPlanCommand(
    Guid RanchId,
    Guid LotId,
    string? Name,
    DateOnly? ValidFrom,
    DateOnly? ValidTo,
    IReadOnlyList<RationItemDraft> Items,
    decimal? TargetDailyGainKilograms,
    string? Notes);
