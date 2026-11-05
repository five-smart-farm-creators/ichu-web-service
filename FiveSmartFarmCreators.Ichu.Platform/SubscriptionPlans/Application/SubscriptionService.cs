using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Ports;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application;

/// <summary>Subscription requests, queries, cancellation and payment requests (US-34, TS-10).</summary>
public sealed class SubscriptionService(
    SubscriptionAuthorization authorization,
    IPlanRepository plans,
    ISubscriptionRepository subscriptions,
    ISubscriptionPlansUnitOfWork unitOfWork,
    IOptions<SubscriptionPlansOptions> options,
    TimeProvider clock)
{
    /// <summary>Registers a Pending request with its activation payment. It never activates the plan or claims a charge.</summary>
    public async Task<SubscriptionRequestResult> RequestAsync(RequestSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var accountId = await authorization.RequireAsync(command.RanchId, SubscriptionPermission.ManageSubscription, cancellationToken);

        var replay = await subscriptions.FindByOperationAsync(command.OperationId, cancellationToken);
        if (replay is not null)
        {
            if (!replay.IsSameRequestAs(command.RanchId, command.PlanId, command.HeadCount, command.QuoteId, command.TermsVersion, accountId))
            {
                throw SubscriptionException.Conflict("A different request already uses this operation identifier.");
            }

            return new SubscriptionRequestResult(false, await DetailsAsync(replay, cancellationToken));
        }

        var currentTerms = options.Value.CurrentTermsVersion?.Trim();
        if (string.IsNullOrEmpty(currentTerms))
        {
            throw SubscriptionException.Unavailable("Subscription requests are unavailable until the current terms version is configured.");
        }

        if (!string.Equals(command.TermsVersion.Trim(), currentTerms, StringComparison.Ordinal))
        {
            throw SubscriptionException.Conflict("The accepted terms are not the current version; review them before requesting again.");
        }

        if (command.HeadCount < 1)
        {
            throw SubscriptionException.InvalidInput("The head count must be a positive integer.");
        }

        var plan = await plans.FindAsync(command.PlanId, cancellationToken)
            ?? throw SubscriptionException.NotFound("The plan does not exist.");
        var now = clock.GetUtcNow();
        if (!plan.IsOfferedAt(now))
        {
            throw SubscriptionException.Conflict("The plan is no longer available at a valid price.");
        }

        if (!plan.Covers(command.HeadCount))
        {
            throw SubscriptionException.Conflict("The head count exceeds the coverage of the plan.");
        }

        var quote = plan.Quote(command.HeadCount, now);
        if (quote.QuoteId != command.QuoteId)
        {
            throw SubscriptionException.Conflict("The estimate changed; review the current quote before requesting again.");
        }

        if (await subscriptions.FindOpenByRanchAsync(command.RanchId, cancellationToken) is not null)
        {
            throw SubscriptionException.Conflict("The ranch already has a pending or active subscription.");
        }

        var subscription = Subscription.Request(command.RanchId, quote, currentTerms, command.AcquisitionSource, accountId, command.OperationId, now);
        subscriptions.Add(subscription);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new SubscriptionRequestResult(true, new SubscriptionDetails(subscription, plan, now));
    }

    public async Task<SubscriptionDetails> GetCurrentAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        await authorization.RequireAsync(ranchId, SubscriptionPermission.ViewSubscription, cancellationToken);
        var subscription = await subscriptions.FindOpenByRanchAsync(ranchId, cancellationToken)
            ?? await subscriptions.FindLatestByRanchAsync(ranchId, cancellationToken)
            ?? throw SubscriptionException.NotFound("The ranch has no subscription.");
        return await DetailsAsync(subscription, cancellationToken);
    }

    public async Task<SubscriptionDetails> GetAsync(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var subscription = await FindAuthorizedAsync(subscriptionId, SubscriptionPermission.ViewSubscription, cancellationToken);
        return await DetailsAsync(subscription, cancellationToken);
    }

    public async Task<SubscriptionDetails> CancelAsync(Guid subscriptionId, string reason, CancellationToken cancellationToken)
    {
        var subscription = await FindAuthorizedAsync(subscriptionId, SubscriptionPermission.ManageSubscription, cancellationToken);
        var outcome = subscription.Cancel(reason, clock.GetUtcNow());
        if (outcome == ChangeOutcome.Conflict)
        {
            throw SubscriptionException.Conflict("The subscription was already cancelled for a different reason.");
        }

        if (outcome == ChangeOutcome.Applied)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return await DetailsAsync(subscription, cancellationToken);
    }

    /// <summary>Requests the payment that activates or renews the subscription; the provider must confirm it separately.</summary>
    public async Task<Payment> RequestPaymentAsync(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var subscription = await FindAuthorizedAsync(subscriptionId, SubscriptionPermission.ManageSubscription, cancellationToken);
        var payment = subscription.RequestPayment(clock.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public async Task<IReadOnlyList<Payment>> GetPaymentsAsync(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var subscription = await FindAuthorizedAsync(subscriptionId, SubscriptionPermission.ViewSubscription, cancellationToken);
        return subscription.Payments.OrderBy(p => p.RequestedAt).ToList();
    }

    private async Task<Subscription> FindAuthorizedAsync(Guid subscriptionId, SubscriptionPermission permission, CancellationToken cancellationToken)
    {
        var subscription = await subscriptions.FindAsync(subscriptionId, cancellationToken)
            ?? throw SubscriptionException.NotFound("The subscription does not exist.");
        await authorization.RequireAsync(subscription.RanchId, permission, cancellationToken);
        return subscription;
    }

    private async Task<SubscriptionDetails> DetailsAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        var plan = await plans.FindAsync(subscription.PlanId, cancellationToken)
            ?? throw SubscriptionException.Unavailable("The plan of this subscription is no longer in the catalog.");
        return new SubscriptionDetails(subscription, plan, clock.GetUtcNow());
    }
}
