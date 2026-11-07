using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;

/// <summary>
///     Reads the plan catalog. Plans are seeded with the schema, so no request creates them.
/// </summary>
public interface IPlanRepository
{
    /// <summary>
    ///     Finds a plan whether or not it is published, without change tracking.
    /// </summary>
    Task<Plan?> FindByIdAsync(PlanId id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the published plans ordered by device limit, without change tracking.
    /// </summary>
    Task<IReadOnlyList<Plan>> ListPublishedAsync(CancellationToken cancellationToken = default);
}
