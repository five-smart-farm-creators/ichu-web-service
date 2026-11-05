using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Persistence;

public sealed class SubscriptionRepository(DbContext context) : ISubscriptionRepository
{
    private DbSet<Subscription> Subscriptions => context.Set<Subscription>();

    public Task<Subscription?> FindAsync(Guid subscriptionId, CancellationToken cancellationToken) =>
        Subscriptions.SingleOrDefaultAsync(s => s.Id == subscriptionId, cancellationToken);

    public Task<Subscription?> FindByOperationAsync(Guid operationId, CancellationToken cancellationToken) =>
        Subscriptions.SingleOrDefaultAsync(s => s.OperationId == operationId, cancellationToken);

    public Task<Subscription?> FindByPaymentAsync(Guid paymentId, CancellationToken cancellationToken) =>
        Subscriptions.SingleOrDefaultAsync(s => s.Payments.Any(p => p.Id == paymentId), cancellationToken);

    public Task<Subscription?> FindOpenByRanchAsync(Guid ranchId, CancellationToken cancellationToken) =>
        Subscriptions.SingleOrDefaultAsync(
            s => s.RanchId == ranchId && (s.Status == SubscriptionStatus.Pending || s.Status == SubscriptionStatus.Active),
            cancellationToken);

    public Task<Subscription?> FindLatestByRanchAsync(Guid ranchId, CancellationToken cancellationToken) =>
        Subscriptions.Where(s => s.RanchId == ranchId).OrderByDescending(s => s.RequestedAt).FirstOrDefaultAsync(cancellationToken);

    public void Add(Subscription subscription) => Subscriptions.Add(subscription);
}
