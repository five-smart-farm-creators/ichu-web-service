using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Events;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Events;

/// <summary>
///     Raised when a retained animal exits the ranch, so that consumers can release its collar and count deaths apart.
/// </summary>
public sealed record CattleDeactivatedEvent(
    Guid RanchId,
    CattleId CattleId,
    ExitReason Reason,
    DateOnly ExitDate,
    DateTimeOffset DeactivatedAt) : IEvent;
