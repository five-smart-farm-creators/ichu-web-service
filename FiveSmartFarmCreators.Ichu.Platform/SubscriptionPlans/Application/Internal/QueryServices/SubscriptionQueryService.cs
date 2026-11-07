using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Internal.QueryServices;

public class SubscriptionQueryService(ISubscriptionRepository subscriptions, ISubscriptionAccessService accessService)
    : ISubscriptionQueryService
{
    /// <inheritdoc />
    public async Task<Result<Subscription>> Handle(GetSubscriptionByRanchQuery query, CancellationToken cancellationToken)
    {
        var access = await accessService.EnsureReadAccessAsync(query.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<Subscription>.Failure(access.DomainError);
        }

        var subscription = await subscriptions.FindCurrentByRanchAsync(query.RanchId, cancellationToken);
        return subscription is null
            ? Result<Subscription>.Failure(SubscriptionPlansErrors.SubscriptionNotFound)
            : Result<Subscription>.Success(subscription);
    }
}
