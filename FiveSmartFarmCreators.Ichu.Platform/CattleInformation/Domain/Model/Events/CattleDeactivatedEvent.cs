using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Events;

/// <summary>
/// Announces a retained animal's exit so consumers can release its collar and distinguish mortality from other exits.
/// </summary>
public sealed record CattleDeactivatedEvent(Guid EventId, DateTimeOffset OccurredAt, Guid RanchId, Guid CattleId, ExitReason Reason, DateOnly ExitDate) : ICattleEvent;
