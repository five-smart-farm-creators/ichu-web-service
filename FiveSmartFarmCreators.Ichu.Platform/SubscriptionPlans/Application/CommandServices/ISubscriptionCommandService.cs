using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.CommandServices;

public interface ISubscriptionCommandService
{
    /// <summary>
    ///     Requests a subscription after checking the ranch permission, the repeated operation, the terms, the quote and
    ///     the existing commitments of the ranch.
    /// </summary>
    Task<Result<SubscriptionRequestOutcome>> Handle(RequestSubscriptionCommand command, CancellationToken cancellationToken);

    Task<Result<Subscription>> Handle(CancelSubscriptionCommand command, CancellationToken cancellationToken);

    /// <summary>
    ///     Activates a pending subscription. The development seeder and, later, the payment confirmation call it.
    /// </summary>
    Task<Result<Subscription>> Handle(ActivateSubscriptionCommand command, CancellationToken cancellationToken);
}
