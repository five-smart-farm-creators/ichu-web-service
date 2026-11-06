namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Events;

/// <summary>
/// Identifies a lifecycle fact whose event identifier remains stable across delivery retries.
/// </summary>
public interface ICattleEvent
{
    Guid EventId { get; }
    /// <summary>
    /// Gets the UTC instant when the fact was recorded, which may differ from a calendar effective date.
    /// </summary>
    DateTimeOffset OccurredAt { get; }
}
