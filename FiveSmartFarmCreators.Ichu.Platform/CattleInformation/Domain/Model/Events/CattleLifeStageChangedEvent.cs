using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Events;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Events;

/// <summary>
///     Raised when an animal moves to another productive stage, effective on a calendar date.
/// </summary>
public sealed record CattleLifeStageChangedEvent(
    Guid RanchId,
    CattleId CattleId,
    LifeStage PreviousStage,
    LifeStage LifeStage,
    DateOnly ValidFrom) : IEvent;
