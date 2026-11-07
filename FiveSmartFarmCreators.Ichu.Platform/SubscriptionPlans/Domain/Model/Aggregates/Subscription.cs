using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Events;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;

/// <summary>
///     Annual contract of a ranch for a plan. The quote is copied when the request is made, so later price changes
///     never alter what the customer agreed to pay.
/// </summary>
public sealed class Subscription : AggregateRoot
{
    public const int MaximumCancellationReasonLength = 200;

    // Entity Framework Core materializes subscriptions without running the factory.
    private Subscription()
    {
    }

    public SubscriptionId Id { get; private set; }

    public Guid RanchId { get; private set; }

    public PlanId PlanId { get; private set; }

    public HeadCount HeadCount { get; private set; }

    public Money AnnualFee { get; private set; } = null!;

    public Money DevicesCost { get; private set; } = null!;

    public Money Total { get; private set; } = null!;

    public SubscriptionStatus Status { get; private set; }

    public string TermsVersion { get; private set; } = string.Empty;

    public Guid OperationId { get; private set; }

    public string? AcquisitionSource { get; private set; }

    public DateTimeOffset RequestedAt { get; private set; }

    public DateTimeOffset? ActivatedAt { get; private set; }

    public DateTimeOffset? ValidUntil { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }

    public string? CancellationReason { get; private set; }

    /// <summary>
    ///     Requests a subscription. It stays pending until a verified commercial confirmation activates it.
    /// </summary>
    public static Subscription Request(
        Guid ranchId,
        PlanQuote quote,
        string termsVersion,
        Guid operationId,
        string? acquisitionSource,
        DateTimeOffset requestedAt)
    {
        var subscription = new Subscription
        {
            Id = new SubscriptionId(Guid.NewGuid()),
            RanchId = ranchId,
            PlanId = quote.PlanId,
            HeadCount = quote.HeadCount,
            AnnualFee = quote.AnnualFee,
            DevicesCost = quote.DevicesCost,
            Total = quote.Total,
            Status = SubscriptionStatus.Pending,
            TermsVersion = termsVersion,
            OperationId = operationId,
            AcquisitionSource = string.IsNullOrWhiteSpace(acquisitionSource) ? null : acquisitionSource.Trim(),
            RequestedAt = requestedAt.ToUniversalTime()
        };
        subscription.AddDomainEvent(new SubscriptionRequestedEvent(subscription.Id, ranchId, quote.PlanId, quote.Total));
        return subscription;
    }

    /// <summary>
    ///     Activates a pending subscription. The coverage lasts one year because the annual fee covers one year.
    /// </summary>
    public Result Activate(DateTimeOffset activatedAt)
    {
        if (Status != SubscriptionStatus.Pending)
        {
            return Result.Failure(SubscriptionPlansErrors.InvalidStatusTransition);
        }

        var activationInstant = activatedAt.ToUniversalTime();
        Status = SubscriptionStatus.Active;
        ActivatedAt = activationInstant;
        ValidUntil = activationInstant.AddYears(1);
        AddDomainEvent(new SubscriptionActivatedEvent(Id, RanchId, ValidUntil.Value));
        return Result.Success();
    }

    /// <summary>
    ///     Cancels a pending or active subscription. The reason is required so that every cancellation can be followed up.
    /// </summary>
    public Result Cancel(string reason, DateTimeOffset cancelledAt)
    {
        var trimmedReason = reason.Trim();
        if (trimmedReason.Length is 0 or > MaximumCancellationReasonLength)
        {
            return Result.Failure(SubscriptionPlansErrors.CancellationReasonRequired);
        }

        if (Status is not (SubscriptionStatus.Pending or SubscriptionStatus.Active))
        {
            return Result.Failure(SubscriptionPlansErrors.InvalidStatusTransition);
        }

        Status = SubscriptionStatus.Cancelled;
        CancelledAt = cancelledAt.ToUniversalTime();
        CancellationReason = trimmedReason;
        AddDomainEvent(new SubscriptionCancelledEvent(Id, RanchId, trimmedReason));
        return Result.Success();
    }

    /// <summary>
    ///     Determines whether the subscription grants coverage at an instant. An expired subscription is inactive even
    ///     before a background job marks it as expired.
    /// </summary>
    public bool IsActiveAt(DateTimeOffset instant) =>
        Status == SubscriptionStatus.Active && ValidUntil is { } validUntil && instant < validUntil;

    /// <summary>
    ///     Determines whether a repeated request asks for this same subscription, so that a retry returns it unchanged.
    /// </summary>
    public bool Matches(RequestSubscriptionCommand command) =>
        RanchId == command.RanchId
        && PlanId == command.PlanId
        && HeadCount.Value == command.HeadCount
        && string.Equals(TermsVersion, command.TermsVersion, StringComparison.Ordinal);
}
