using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;

/// <summary>
///     Requests an effective-dated stage change guarded by the animal's expected version.
/// </summary>
public sealed record ChangeLifeStageCommand(
    Guid RanchId,
    CattleId CattleId,
    LifeStage LifeStage,
    DateOnly ValidFrom,
    Guid ExpectedVersion);
