using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Events;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Persistence.EntityFrameworkCore;

/// <summary>A durable event awaiting delivery by the host's integration relay.</summary>
public sealed class CattleIntegrationEvent
{
    private CattleIntegrationEvent()
    {
    }

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
    public DateTimeOffset? PublishedAt { get; private set; }

    /// <summary>Call only after all required consumers acknowledge this stable event identifier.</summary>
    public void MarkPublished(DateTimeOffset publishedAt)
    {
        if (publishedAt.Offset != TimeSpan.Zero || publishedAt < OccurredAt)
        {
            throw new ArgumentException("Publication time must be UTC and cannot precede the event.", nameof(publishedAt));
        }

        PublishedAt ??= publishedAt;
    }
}
