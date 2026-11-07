using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;

/// <summary>
///     Persists subscriptions. Only the lookup used by state changes is tracked, so that the unit of work saves them.
/// </summary>
public interface ISubscriptionRepository
{
    /// <summary>
    ///     Stages a new subscription for saving by the unit of work.
    /// </summary>
    Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds a subscription with change tracking, so that its state changes are saved.
    /// </summary>
    Task<Subscription?> FindByIdAsync(SubscriptionId id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the subscription requested with an operation identifier, so that a repeated request can return it.
    /// </summary>
    Task<Subscription?> FindByOperationIdAsync(Guid operationId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the latest pending or active subscription of a ranch, without change tracking.
    /// </summary>
    Task<Subscription?> FindCurrentByRanchAsync(Guid ranchId, CancellationToken cancellationToken = default);
}
