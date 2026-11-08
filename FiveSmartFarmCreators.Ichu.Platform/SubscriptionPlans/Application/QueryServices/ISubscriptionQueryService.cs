using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Queries;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.QueryServices;

public interface ISubscriptionQueryService
{
    /// <summary>
    ///     Reads the pending or active subscription of a ranch after checking the read permission.
    /// </summary>
    Task<Result<Subscription>> Handle(GetSubscriptionByRanchQuery query, CancellationToken cancellationToken);
}
