using Cortex.Mediator;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Mediator.Cortex.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;

/// <summary>
///     Publishes the domain events of aggregates after their changes are saved, so that handlers never react to a
///     change that was rolled back.
/// </summary>
public sealed class DispatchDomainEventsInterceptor(IServiceScopeFactory scopeFactory) : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        DispatchDomainEventsAsync(eventData.Context, CancellationToken.None).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    /// <inheritdoc />
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(DbContext? context, CancellationToken cancellationToken)
    {
        if (context is null)
        {
            return;
        }

        var aggregateRoots = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(entry => entry.Entity)
            .Where(aggregateRoot => aggregateRoot.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(aggregateRoot => aggregateRoot.DomainEvents)
            .ToList();

        foreach (var aggregateRoot in aggregateRoots)
        {
            aggregateRoot.ClearDomainEvents();
        }

        if (domainEvents.Count == 0)
        {
            return;
        }

        // A handler gets its own scope because the context that just saved may not be reused while its save is still running.
        await using var scope = scopeFactory.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        foreach (var domainEvent in domainEvents)
        {
            await DomainEventPublisher.PublishAsync(mediator, domainEvent, cancellationToken);
        }
    }
}
