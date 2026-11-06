using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Events;

public sealed record CattleDeactivatedEvent(Guid EventId, DateTimeOffset OccurredAt, Guid RanchId, Guid CattleId, ExitReason Reason, DateOnly ExitDate) : ICattleEvent;
