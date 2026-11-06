using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Events;

public sealed record CattleLifeStageChangedEvent(Guid EventId, DateTimeOffset OccurredAt, Guid RanchId, Guid CattleId, LifeStage PreviousStage, LifeStage LifeStage, DateOnly ValidFrom) : ICattleEvent;
