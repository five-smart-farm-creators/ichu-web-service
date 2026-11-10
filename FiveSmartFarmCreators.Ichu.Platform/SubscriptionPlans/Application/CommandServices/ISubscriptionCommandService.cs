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
    ///     Activates a pending subscription. The development seeder calls it, and a confirmed payment applies the same activation.
    /// </summary>
    Task<Result<Subscription>> Handle(ActivateSubscriptionCommand command, CancellationToken cancellationToken);

    /// <summary>
    ///     Starts the hosted checkout of a pending subscription after checking the ranch permission and the status. The
    ///     subscription stays pending until a confirmed payment activates it.
    /// </summary>
    Task<Result<CheckoutSession>> Handle(StartSubscriptionCheckoutCommand command, CancellationToken cancellationToken);

    /// <summary>
    ///     Verifies a provider event and applies the payment that it confirms. Events that change no subscription are
    ///     reported as ignored.
    /// </summary>
    Task<Result<Subscription>> Handle(ReceivePaymentEventCommand command, CancellationToken cancellationToken);

    /// <summary>
    ///     Applies a confirmed payment. The subscription is activated only when the paid amount and currency equal its total.
    /// </summary>
    Task<Result<Subscription>> Handle(ConfirmSubscriptionPaymentCommand command, CancellationToken cancellationToken);
}
