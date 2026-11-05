using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;

/// <summary>
/// The subscription of a ranch. A request starts as Pending with a pending activation payment; only a verified
/// accepted payment activates it or extends its paid period. Cancellation keeps the payment history.
/// </summary>
public sealed class Subscription
{
    public const int MaximumTermsVersionLength = 40;
    public const int MaximumAcquisitionSourceLength = 80;
    public const int MaximumCancellationReasonLength = 200;

    private readonly List<Payment> _payments = [];

    public Guid Id { get; private set; }
    public Guid RanchId { get; private set; }
    public Guid PlanId { get; private set; }
    public int PlanPricingVersion { get; private set; }
    public int HeadCount { get; private set; }
    public BillingPeriod BillingPeriod { get; private set; }
    public Money RecurringAmount { get; private set; } = null!;
    public Guid QuoteId { get; private set; }
    public string TermsVersion { get; private set; } = string.Empty;
    public string? AcquisitionSource { get; private set; }
    public Guid RequestedBy { get; private set; }
    public Guid OperationId { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTimeOffset RequestedAt { get; private set; }
    public DateTimeOffset? ActivatedAt { get; private set; }
    public DateTimeOffset? PaidUntil { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public int Version { get; private set; }
    public IReadOnlyCollection<Payment> Payments => _payments;

    private Subscription()
    {
    }

    private Subscription(Guid ranchId, PlanQuote quote, string termsVersion, string? acquisitionSource, Guid requestedBy, Guid operationId, DateTimeOffset requestedAt)
    {
        if (ranchId == Guid.Empty || requestedBy == Guid.Empty || operationId == Guid.Empty)
        {
            throw SubscriptionException.InvalidInput("Ranch, requester and operation identifiers are required.");
        }

        Id = Guid.NewGuid();
        RanchId = ranchId;
        PlanId = quote.PlanId;
        PlanPricingVersion = quote.PricingVersion;
        HeadCount = quote.HeadCount;
        BillingPeriod = quote.BillingPeriod;
        RecurringAmount = quote.RecurringAmount;
        QuoteId = quote.QuoteId;
        TermsVersion = RequiredText.Normalize(termsVersion, MaximumTermsVersionLength, "terms version");
        AcquisitionSource = string.IsNullOrWhiteSpace(acquisitionSource)
            ? null
            : RequiredText.Normalize(acquisitionSource, MaximumAcquisitionSourceLength, "acquisition source");
        RequestedBy = requestedBy;
        OperationId = operationId;
        Status = SubscriptionStatus.Pending;
        RequestedAt = requestedAt.ToUniversalTime();
        Version = 1;
        _payments.Add(new Payment(PaymentPurpose.Activation, RecurringAmount, RequestedAt));
    }

    /// <summary>Registers a subscription request from a current quote. It does not confirm any payment.</summary>
    public static Subscription Request(Guid ranchId, PlanQuote quote, string termsVersion, string? acquisitionSource, Guid requestedBy, Guid operationId, DateTimeOffset requestedAt) =>
        new(ranchId, quote, termsVersion, acquisitionSource, requestedBy, operationId, requestedAt);

    public SubscriptionStatus StatusAt(DateTimeOffset now) =>
        Status == SubscriptionStatus.Active && now >= PaidUntil ? SubscriptionStatus.Expired : Status;

    public bool IsEntitledAt(DateTimeOffset now) => StatusAt(now) == SubscriptionStatus.Active;

    public bool IsSameRequestAs(Guid ranchId, Guid planId, int headCount, Guid quoteId, string termsVersion, Guid requestedBy) =>
        RanchId == ranchId &&
        PlanId == planId &&
        HeadCount == headCount &&
        QuoteId == quoteId &&
        TermsVersion == termsVersion.Trim() &&
        RequestedBy == requestedBy;

    /// <summary>Requests the payment that activates a pending subscription or renews an active or expired one.</summary>
    public Payment RequestPayment(DateTimeOffset now)
    {
        if (Status == SubscriptionStatus.Cancelled)
        {
            throw SubscriptionException.Conflict("A cancelled subscription cannot receive new payments.");
        }

        if (_payments.Any(p => p.Status == PaymentStatus.Pending))
        {
            throw SubscriptionException.Conflict("A payment is already pending for this subscription.");
        }

        var purpose = Status == SubscriptionStatus.Pending ? PaymentPurpose.Activation : PaymentPurpose.Renewal;
        var payment = new Payment(purpose, RecurringAmount, now);
        _payments.Add(payment);
        Version++;
        return payment;
    }

    public ChangeOutcome ConfirmPayment(Guid paymentId, string externalReference, Money confirmedAmount, DateTimeOffset processedAt)
    {
        var payment = FindPayment(paymentId);
        if (payment.Status == PaymentStatus.Accepted && payment.ExternalReference == externalReference.Trim())
        {
            return ChangeOutcome.AlreadyApplied;
        }

        if (payment.Status != PaymentStatus.Pending || confirmedAmount != payment.Amount)
        {
            return ChangeOutcome.Conflict;
        }

        var at = processedAt.ToUniversalTime();
        payment.MarkAccepted(externalReference, at);

        // A payment accepted after cancellation is recorded but does not reactivate the subscription.
        if (Status != SubscriptionStatus.Cancelled)
        {
            var periodStart = PaidUntil is { } paidUntil && paidUntil > at ? paidUntil : at;
            PaidUntil = AddPeriod(periodStart);
            ActivatedAt ??= at;
            Status = SubscriptionStatus.Active;
        }

        Version++;
        return ChangeOutcome.Applied;
    }

    public ChangeOutcome RejectPayment(Guid paymentId, string externalReference, string? reason, DateTimeOffset processedAt)
    {
        var payment = FindPayment(paymentId);
        if (payment.Status == PaymentStatus.Rejected && payment.ExternalReference == externalReference.Trim())
        {
            return ChangeOutcome.AlreadyApplied;
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return ChangeOutcome.Conflict;
        }

        payment.MarkRejected(externalReference, reason, processedAt);
        Version++;
        return ChangeOutcome.Applied;
    }

    /// <summary>Cancels the subscription immediately; entitlements end and the payment history is preserved.</summary>
    public ChangeOutcome Cancel(string reason, DateTimeOffset now)
    {
        var normalized = RequiredText.Normalize(reason, MaximumCancellationReasonLength, "cancellation reason");
        if (Status == SubscriptionStatus.Cancelled)
        {
            return CancellationReason == normalized ? ChangeOutcome.AlreadyApplied : ChangeOutcome.Conflict;
        }

        Status = SubscriptionStatus.Cancelled;
        CancelledAt = now.ToUniversalTime();
        CancellationReason = normalized;
        Version++;
        return ChangeOutcome.Applied;
    }

    private Payment FindPayment(Guid paymentId) =>
        _payments.SingleOrDefault(p => p.Id == paymentId)
        ?? throw SubscriptionException.NotFound("The payment does not belong to this subscription.");

    private DateTimeOffset AddPeriod(DateTimeOffset start) => BillingPeriod switch
    {
        BillingPeriod.Monthly => start.AddMonths(1),
        BillingPeriod.Annual => start.AddYears(1),
        _ => throw new ArgumentOutOfRangeException(nameof(BillingPeriod), BillingPeriod, null)
    };
}
