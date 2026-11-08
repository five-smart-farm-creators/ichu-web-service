using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Events;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Events;

/// <summary>
///     Raised when an animal enters the ranch inventory with its identity and the stage known at registration.
/// </summary>
public sealed record CattleRegisteredEvent(
    Guid RanchId,
    CattleId CattleId,
    string EarTag,
    Breed Breed,
    Sex Sex,
    DateOnly BirthDate,
    LifeStage LifeStage) : IEvent;
