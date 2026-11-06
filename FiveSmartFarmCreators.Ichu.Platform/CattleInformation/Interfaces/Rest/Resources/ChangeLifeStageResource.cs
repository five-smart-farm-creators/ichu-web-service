using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

/// <summary>
/// Supplies a calendar effective date and the animal version for a history-preserving stage change.
/// </summary>
public sealed record ChangeLifeStageResource(LifeStage LifeStage, DateOnly ValidFrom, Guid ExpectedVersion);
