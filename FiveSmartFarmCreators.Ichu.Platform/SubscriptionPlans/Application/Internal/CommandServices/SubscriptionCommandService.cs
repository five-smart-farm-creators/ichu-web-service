using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Internal.CommandServices;

/// <summary>
///     Requests, cancels, pays and activates subscriptions. It checks permissions and commercial rules here and leaves the
///     state transitions to the aggregate; a rejected request saves nothing.
/// </summary>
public class SubscriptionCommandService(
    ISubscriptionAccessService accessService,
    IPlanRepository plans,
    ISubscriptionRepository subscriptions,
    IUnitOfWork unitOfWork,
    IPaymentGateway payments,
    IOptions<SubscriptionPlansOptions> options,
    TimeProvider clock) : ISubscriptionCommandService
{
    /// <inheritdoc />
    public async Task<Result<SubscriptionRequestOutcome>> Handle(RequestSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var access = await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<SubscriptionRequestOutcome>.Failure(access.DomainError);
        }

        var stored = await subscriptions.FindByOperationIdAsync(command.OperationId, cancellationToken);
        if (stored is not null)
        {
            return stored.Matches(command)
                ? Result<SubscriptionRequestOutcome>.Success(new SubscriptionRequestOutcome(stored, Created: false))
                : Result<SubscriptionRequestOutcome>.Failure(SubscriptionPlansErrors.OperationConflict);
        }

        if (!string.Equals(command.TermsVersion, options.Value.CurrentTermsVersion, StringComparison.Ordinal))
        {
            return Result<SubscriptionRequestOutcome>.Failure(SubscriptionPlansErrors.TermsVersionOutdated);
        }

        var headCount = HeadCount.Create(command.HeadCount);
        if (headCount.IsFailure)
        {
            return Result<SubscriptionRequestOutcome>.Failure(headCount.DomainError);
        }

        var plan = await plans.FindByIdAsync(command.PlanId, cancellationToken);
        if (plan is null)
        {
            return Result<SubscriptionRequestOutcome>.Failure(SubscriptionPlansErrors.PlanNotFound);
        }

        var quote = plan.EstimateFor(headCount.Value!);
        if (quote.IsFailure)
        {
            return Result<SubscriptionRequestOutcome>.Failure(quote.DomainError);
        }

        if (command.ExpectedTotal != quote.Value!.Total.Amount)
        {
            return Result<SubscriptionRequestOutcome>.Failure(SubscriptionPlansErrors.QuoteChanged);
        }

        if (await subscriptions.FindCurrentByRanchAsync(command.RanchId, cancellationToken) is not null)
        {
            return Result<SubscriptionRequestOutcome>.Failure(SubscriptionPlansErrors.SubscriptionAlreadyExists);
        }

        var subscription = Subscription.Request(command.RanchId, quote.Value!, command.TermsVersion, command.OperationId,
            command.AcquisitionSource, clock.GetUtcNow());
        await subscriptions.AddAsync(subscription, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<SubscriptionRequestOutcome>.Success(new SubscriptionRequestOutcome(subscription, Created: true));
    }

    /// <inheritdoc />
    public async Task<Result<Subscription>> Handle(CancelSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var access = await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<Subscription>.Failure(access.DomainError);
        }

        // The ranch is checked before the lookup, and a subscription of another ranch is reported as missing, so identifiers cannot be probed.
        var subscription = await subscriptions.FindByIdAsync(command.SubscriptionId, cancellationToken);
        if (subscription is null || subscription.RanchId != command.RanchId)
        {
            return Result<Subscription>.Failure(SubscriptionPlansErrors.SubscriptionNotFound);
        }

        var cancellation = subscription.Cancel(command.Reason, clock.GetUtcNow());
        if (cancellation.IsFailure)
        {
            return Result<Subscription>.Failure(cancellation.DomainError);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Subscription>.Success(subscription);
    }

    /// <inheritdoc />
    public async Task<Result<Subscription>> Handle(ActivateSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var subscription = await subscriptions.FindByIdAsync(command.SubscriptionId, cancellationToken);
        if (subscription is null)
        {
            return Result<Subscription>.Failure(SubscriptionPlansErrors.SubscriptionNotFound);
        }

        return await ActivateAsync(subscription, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<CheckoutSession>> Handle(StartSubscriptionCheckoutCommand command, CancellationToken cancellationToken)
    {
        var access = await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<CheckoutSession>.Failure(access.DomainError);
        }

        // A subscription of another ranch is reported as missing, so identifiers cannot be probed.
        var subscription = await subscriptions.FindByIdAsync(command.SubscriptionId, cancellationToken);
        if (subscription is null || subscription.RanchId != command.RanchId)
        {
            return Result<CheckoutSession>.Failure(SubscriptionPlansErrors.SubscriptionNotFound);
        }

        if (subscription.Status != SubscriptionStatus.Pending)
        {
            return Result<CheckoutSession>.Failure(SubscriptionPlansErrors.SubscriptionNotPending);
        }

        var plan = await plans.FindByIdAsync(subscription.PlanId, cancellationToken);
        if (plan is null)
        {
            return Result<CheckoutSession>.Failure(SubscriptionPlansErrors.PlanNotFound);
        }

        return await payments.CreateCheckoutSessionAsync(subscription, plan.Name, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<Subscription>> Handle(ReceivePaymentEventCommand command, CancellationToken cancellationToken)
    {
        var confirmation = payments.ReadConfirmation(command.Payload, command.SignatureHeader);
        if (confirmation.IsFailure)
        {
            return Result<Subscription>.Failure(confirmation.DomainError);
        }

        return await Handle(new ConfirmSubscriptionPaymentCommand(confirmation.Value!), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<Subscription>> Handle(ConfirmSubscriptionPaymentCommand command, CancellationToken cancellationToken)
    {
        var confirmation = command.Confirmation;
        var subscription = await subscriptions.FindByIdAsync(confirmation.SubscriptionId, cancellationToken);
        if (subscription is null)
        {
            return Result<Subscription>.Failure(SubscriptionPlansErrors.SubscriptionNotFound);
        }

        // Stripe redelivers events until they are acknowledged, so a repeat for an active subscription succeeds unchanged.
        if (subscription.Status == SubscriptionStatus.Active || !confirmation.Paid)
        {
            return Result<Subscription>.Success(subscription);
        }

        if (confirmation.AmountMinor != subscription.Total.ToMinorUnits() || confirmation.Currency != subscription.Total.Currency)
        {
            return Result<Subscription>.Failure(SubscriptionPlansErrors.PaymentAmountMismatch);
        }

        return await ActivateAsync(subscription, cancellationToken);
    }

    private async Task<Result<Subscription>> ActivateAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        var activation = subscription.Activate(clock.GetUtcNow());
        if (activation.IsFailure)
        {
            return Result<Subscription>.Failure(activation.DomainError);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Subscription>.Success(subscription);
    }
}
