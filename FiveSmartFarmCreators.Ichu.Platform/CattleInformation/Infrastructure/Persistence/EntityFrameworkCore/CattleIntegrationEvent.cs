using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Events;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Persistence.EntityFrameworkCore;

/// <summary>
/// Retains a lifecycle event for delivery by the host's relay independently of the originating request.
/// </summary>
public sealed class CattleIntegrationEvent
{
    private CattleIntegrationEvent()
    {
    }

    /// <summary>
    /// Serializes the concrete event using its stable identity, camelCase fields and numeric enum values.
    /// </summary>
    public CattleIntegrationEvent(ICattleEvent domainEvent)
    {
        Id = domainEvent.EventId;
        OccurredAt = domainEvent.OccurredAt;
        EventType = domainEvent.GetType().Name;
        Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    public Guid Id { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    /// <summary>
    /// Gets the first acknowledged delivery time in UTC, or null while delivery remains pending.
    /// </summary>
    public DateTimeOffset? PublishedAt { get; private set; }

    /// <summary>
    /// Marks delivery after all required consumers acknowledge the event, retaining the first publication time.
    /// </summary>
    /// <param name="publishedAt">The UTC acknowledgement instant, no earlier than the event's occurrence.</param>
    /// <exception cref="ArgumentException">The timestamp has a nonzero offset or precedes the event.</exception>
    public void MarkPublished(DateTimeOffset publishedAt)
    {
        if (publishedAt.Offset != TimeSpan.Zero || publishedAt < OccurredAt)
        {
            throw new ArgumentException("Publication time must be UTC and cannot precede the event.", nameof(publishedAt));
        }

        PublishedAt ??= publishedAt;
    }
}
