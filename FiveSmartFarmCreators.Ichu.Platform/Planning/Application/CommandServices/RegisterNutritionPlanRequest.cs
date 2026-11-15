using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.CommandServices;

public sealed record RegisterNutritionPlanRequest(
    Guid LotId,
    string? Name,
    DateOnly? ValidFrom,
    DateOnly? ValidTo,
    IReadOnlyList<RationItemDraft> Items,
    decimal? TargetDailyGainKilograms,
    string? Notes);
