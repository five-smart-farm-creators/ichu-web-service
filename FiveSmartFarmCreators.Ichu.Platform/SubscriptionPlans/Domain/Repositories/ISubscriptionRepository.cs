using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;

public interface ISubscriptionRepository
{
    Task<Subscription?> FindAsync(Guid subscriptionId, CancellationToken cancellationToken);

    Task<Subscription?> FindByOperationAsync(Guid operationId, CancellationToken cancellationToken);

    Task<Subscription?> FindByPaymentAsync(Guid paymentId, CancellationToken cancellationToken);

    /// <summary>Finds the pending or active subscription of a ranch; a ranch has at most one.</summary>
    Task<Subscription?> FindOpenByRanchAsync(Guid ranchId, CancellationToken cancellationToken);

    Task<Subscription?> FindLatestByRanchAsync(Guid ranchId, CancellationToken cancellationToken);

    void Add(Subscription subscription);
}
