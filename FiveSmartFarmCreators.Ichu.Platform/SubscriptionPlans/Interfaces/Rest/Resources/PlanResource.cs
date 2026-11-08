namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;

public sealed record PlanResource(
    Guid Id,
    string Tier,
    string Name,
    int DeviceLimit,
    MoneyResource AnnualFee,
    MoneyResource DeviceUnitPrice,
    IReadOnlyList<string> Features);
