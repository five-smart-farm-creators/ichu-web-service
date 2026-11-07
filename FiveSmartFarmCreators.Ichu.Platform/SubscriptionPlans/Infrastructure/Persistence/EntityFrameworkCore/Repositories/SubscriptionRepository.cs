using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class SubscriptionRepository(AppDbContext context) : ISubscriptionRepository
{
    private DbSet<Subscription> Subscriptions => context.Set<Subscription>();

    /// <inheritdoc />
    public async Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        await Subscriptions.AddAsync(subscription, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Subscription?> FindByIdAsync(SubscriptionId id, CancellationToken cancellationToken = default) =>
        await Subscriptions.FirstOrDefaultAsync(subscription => subscription.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<Subscription?> FindByOperationIdAsync(Guid operationId, CancellationToken cancellationToken = default) =>
        await Subscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(subscription => subscription.OperationId == operationId, cancellationToken);

    /// <inheritdoc />
    public async Task<Subscription?> FindCurrentByRanchAsync(Guid ranchId, CancellationToken cancellationToken = default) =>
        await Subscriptions
            .AsNoTracking()
            .Where(subscription => subscription.RanchId == ranchId
                && (subscription.Status == SubscriptionStatus.Pending || subscription.Status == SubscriptionStatus.Active))
            .OrderByDescending(subscription => subscription.RequestedAt)
            .FirstOrDefaultAsync(cancellationToken);
}
