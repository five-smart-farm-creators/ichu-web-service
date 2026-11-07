using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;

/// <summary>
///     Catalog plan that sets the annual fee, the price per device and the device limit a ranch can subscribe to.
/// </summary>
public sealed class Plan : AggregateRoot
{
    private readonly List<IncludedFeature> _features = [];

    // Entity Framework Core materializes plans without running the domain constructor.
    private Plan()
    {
    }

    public Plan(
        PlanId id,
        PlanTier tier,
        string name,
        DeviceLimit deviceLimit,
        Money annualFee,
        Money deviceUnitPrice,
        IEnumerable<FeatureCode> features,
        bool isPublished)
    {
        Id = id;
        Tier = tier;
        Name = name;
        DeviceLimit = deviceLimit;
        AnnualFee = annualFee;
        DeviceUnitPrice = deviceUnitPrice;
        IsPublished = isPublished;
        _features.AddRange(features.Select(code => new IncludedFeature(code)));
    }

    public PlanId Id { get; private set; }

    public PlanTier Tier { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DeviceLimit DeviceLimit { get; private set; }

    public Money AnnualFee { get; private set; } = null!;

    public Money DeviceUnitPrice { get; private set; } = null!;

    public bool IsPublished { get; private set; }

    public IReadOnlyCollection<FeatureCode> Features => _features.Select(feature => feature.Code).ToArray();

    /// <summary>
    ///     Quotes the annual price for a head count. An unpublished plan cannot be sold, and a head count above the
    ///     device limit would leave collars without a plan.
    /// </summary>
    public Result<PlanQuote> EstimateFor(HeadCount headCount)
    {
        if (!IsPublished)
        {
            return Result<PlanQuote>.Failure(SubscriptionPlansErrors.PlanUnavailable);
        }

        if (!DeviceLimit.Covers(headCount.Value))
        {
            return Result<PlanQuote>.Failure(SubscriptionPlansErrors.HeadCountExceedsPlan);
        }

        var devicesCost = DeviceUnitPrice.Multiply(headCount.Value);
        var total = AnnualFee.Add(devicesCost);
        return Result<PlanQuote>.Success(new PlanQuote(Id, headCount, AnnualFee, DeviceUnitPrice, devicesCost, total));
    }

    public bool Includes(FeatureCode feature) => _features.Any(included => included.Code == feature);

    /// <summary>
    ///     One feature granted by a plan. It is an owned collection so that each plan lists its features in its own table.
    /// </summary>
    internal sealed class IncludedFeature
    {
        // Entity Framework Core materializes feature rows without running the constructor.
        private IncludedFeature()
        {
        }

        internal IncludedFeature(FeatureCode code)
        {
            Code = code;
        }

        public FeatureCode Code { get; private set; }
    }
}
