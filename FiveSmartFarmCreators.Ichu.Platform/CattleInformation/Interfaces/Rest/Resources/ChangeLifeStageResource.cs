using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

public sealed record ChangeLifeStageResource(LifeStage LifeStage, DateOnly ValidFrom, Guid ExpectedVersion);
