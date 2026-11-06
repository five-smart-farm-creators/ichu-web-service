using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;

public sealed record ChangeLifeStageCommand(Guid RanchId, Guid CattleId, LifeStage LifeStage, DateOnly ValidFrom, Guid ExpectedVersion);
