using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;

/// <summary>
///     Changes only supplied animal fields, with an effective date required for a productive-stage change.
/// </summary>
public sealed record PatchCattleCommand(
    CattleId CattleId,
    Guid ExpectedVersion,
    string? EarTag = null,
    Breed? Breed = null,
    Sex? Sex = null,
    DateOnly? BirthDate = null,
    LifeStage? LifeStage = null,
    DateOnly? ValidFrom = null);
