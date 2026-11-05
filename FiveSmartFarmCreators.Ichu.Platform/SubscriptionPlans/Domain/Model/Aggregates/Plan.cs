using System.Security.Cryptography;
using System.Text;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;

/// <summary>
/// A commercial plan: head-count coverage, device limit, enabled features and per-head pricing. Prices are
/// published with an expiry, and every price change must increase the pricing version so earlier quotes expire.
/// </summary>
public sealed class Plan
{
    public const int MaximumCodeLength = 40;
    public const int MaximumNameLength = 80;

    private readonly List<PlanFeature> _features = [];

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public BillingPeriod BillingPeriod { get; private set; }
    public Money PricePerHead { get; private set; } = null!;
    public Money HardwareUnitPrice { get; private set; } = null!;
    public int MinimumHeadCount { get; private set; }
    public int? MaximumHeadCount { get; private set; }
    public int MaximumDevices { get; private set; }
    public bool IsPublished { get; private set; }
    public int PricingVersion { get; private set; }
    public DateTimeOffset PricesValidUntil { get; private set; }
    public IReadOnlyCollection<PlanFeature> Features => _features;

    private Plan()
    {
    }

    public Plan(
        string code,
        string name,
        BillingPeriod billingPeriod,
        Money pricePerHead,
        Money hardwareUnitPrice,
        int minimumHeadCount,
        int? maximumHeadCount,
        int maximumDevices,
        IEnumerable<FeatureCode> features,
        bool isPublished,
        int pricingVersion,
        DateTimeOffset pricesValidUntil)
    {
        Code = RequiredText.Normalize(code, MaximumCodeLength, "plan code").ToLowerInvariant();
        Name = RequiredText.Normalize(name, MaximumNameLength, "plan name");
        if (pricePerHead.Currency != hardwareUnitPrice.Currency)
        {
            throw SubscriptionException.InvalidInput("Subscription and hardware prices must use the same currency.");
        }

        if (minimumHeadCount < 1 || maximumHeadCount < minimumHeadCount)
        {
            throw SubscriptionException.InvalidInput("Plan coverage requires a positive minimum not greater than its maximum.");
        }

        if (maximumDevices < 0 || pricingVersion < 1)
        {
            throw SubscriptionException.InvalidInput("The device limit cannot be negative and the pricing version must be positive.");
        }

        Id = Guid.NewGuid();
        BillingPeriod = billingPeriod;
        PricePerHead = pricePerHead;
        HardwareUnitPrice = hardwareUnitPrice;
        MinimumHeadCount = minimumHeadCount;
        MaximumHeadCount = maximumHeadCount;
        MaximumDevices = maximumDevices;
        IsPublished = isPublished;
        PricingVersion = pricingVersion;
        PricesValidUntil = pricesValidUntil.ToUniversalTime();
        _features.AddRange(features.Distinct().Select(f => new PlanFeature(f)));
    }

    public bool Covers(int headCount) => headCount >= MinimumHeadCount && (MaximumHeadCount is null || headCount <= MaximumHeadCount);

    public bool IsOfferedAt(DateTimeOffset now) => IsPublished && now < PricesValidUntil;

    public bool Includes(FeatureCode feature) => _features.Any(f => f.Code == feature);

    /*
     * The hardware estimate assumes one collar per covered head; this assumption is pending commercial
     * confirmation. It is reported separately because it is not part of the recurring subscription charge.
     */
    public PlanQuote Quote(int headCount, DateTimeOffset now)
    {
        if (headCount < 1)
        {
            throw SubscriptionException.InvalidInput("The head count must be a positive integer.");
        }

        if (!IsOfferedAt(now))
        {
            throw SubscriptionException.Conflict("The plan is not currently offered at a valid price.");
        }

        if (!Covers(headCount))
        {
            throw SubscriptionException.InvalidInput("The head count is outside the coverage of the plan.");
        }

        return new PlanQuote(
            QuoteIdFor(headCount),
            Id,
            PricingVersion,
            headCount,
            BillingPeriod,
            PricePerHead,
            PricePerHead.Multiply(headCount),
            HardwareUnitPrice,
            HardwareUnitPrice.Multiply(headCount),
            PricesValidUntil);
    }

    // The quote identifier binds the plan, its pricing version and the head count without storing anonymous quotes.
    private Guid QuoteIdFor(int headCount)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{Id:N}|{PricingVersion}|{headCount}"));
        return new Guid(hash.AsSpan(0, 16));
    }
}
