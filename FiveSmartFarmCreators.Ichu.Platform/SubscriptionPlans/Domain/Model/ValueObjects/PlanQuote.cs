namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

/// <summary>
///     Annual price of a plan for a head count, broken down into the annual fee and the devices cost.
/// </summary>
public sealed record PlanQuote(
    PlanId PlanId,
    HeadCount HeadCount,
    Money AnnualFee,
    Money DeviceUnitPrice,
    Money DevicesCost,
    Money Total);
