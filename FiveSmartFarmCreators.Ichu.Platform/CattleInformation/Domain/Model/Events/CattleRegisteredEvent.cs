using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Events;

/// <summary>
/// Captures an animal's identity and current stage when it enters the ranch's registered inventory.
/// </summary>
public sealed record CattleRegisteredEvent(Guid EventId, DateTimeOffset OccurredAt, Guid RanchId, Guid CattleId, string EarTag, Breed Breed, Sex Sex, DateOnly BirthDate, LifeStage LifeStage) : ICattleEvent;
