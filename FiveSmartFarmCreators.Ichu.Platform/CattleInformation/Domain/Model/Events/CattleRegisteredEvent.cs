using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Events;

public sealed record CattleRegisteredEvent(Guid EventId, DateTimeOffset OccurredAt, Guid RanchId, Guid CattleId, string EarTag, Breed Breed, Sex Sex, DateOnly BirthDate, LifeStage LifeStage) : ICattleEvent;
