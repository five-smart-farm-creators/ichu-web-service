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
///     Requests, cancels and activates subscriptions. It checks permissions and commercial rules here and leaves the
///     state transitions to the aggregate; a rejected request saves nothing.
/// </summary>
public class SubscriptionCommandService(
    ISubscriptionAccessService accessService,
    IPlanRepository plans,
    ISubscriptionRepository subscriptions,
    IUnitOfWork unitOfWork,
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
        var subscription = await subscriptions.FindByIdAsync(command.SubscriptionId, cancellationToken);
        if (subscription is null)
        {
            return Result<Subscription>.Failure(SubscriptionPlansErrors.SubscriptionNotFound);
        }

        var access = await accessService.EnsureManageAccessAsync(subscription.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<Subscription>.Failure(access.DomainError);
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

        var activation = subscription.Activate(clock.GetUtcNow());
        if (activation.IsFailure)
        {
            return Result<Subscription>.Failure(activation.DomainError);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Subscription>.Success(subscription);
    }
}
