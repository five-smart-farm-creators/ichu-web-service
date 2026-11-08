using System.Reflection;
using Cortex.Mediator;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Events;

namespace FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Mediator.Cortex.Configuration;

/// <summary>
///     Publishes domain events by their runtime type. Cortex selects handlers from the static type, and aggregates
///     expose their events as <see cref="IEvent"/>, so publishing with that type would skip every handler.
/// </summary>
public static class DomainEventPublisher
{
    private static readonly MethodInfo PublishMethod = typeof(IMediator)
        .GetMethods()
        .Single(method => method.Name == nameof(IMediator.PublishAsync) && method.IsGenericMethodDefinition);

    public static Task PublishAsync(IMediator mediator, IEvent domainEvent, CancellationToken cancellationToken)
    {
        var publish = PublishMethod.MakeGenericMethod(domainEvent.GetType());
        return (Task)publish.Invoke(mediator, [domainEvent, cancellationToken])!;
    }
}
