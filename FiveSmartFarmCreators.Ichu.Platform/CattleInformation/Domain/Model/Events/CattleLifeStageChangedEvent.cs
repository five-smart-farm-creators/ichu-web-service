using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Events;

/// <summary>
/// Records a productive-stage transition with its calendar effective date, separate from the event timestamp.
/// </summary>
public sealed record CattleLifeStageChangedEvent(Guid EventId, DateTimeOffset OccurredAt, Guid RanchId, Guid CattleId, LifeStage PreviousStage, LifeStage LifeStage, DateOnly ValidFrom) : ICattleEvent;
