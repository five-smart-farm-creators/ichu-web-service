using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;

public interface IPlanRepository
{
    Task<Plan?> FindAsync(Guid planId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Plan>> FindPublishedAsync(CancellationToken cancellationToken);
}
