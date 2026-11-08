namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;

public sealed record PlanQuoteResource(
    Guid PlanId,
    int HeadCount,
    MoneyResource AnnualFee,
    MoneyResource DeviceUnitPrice,
    MoneyResource DevicesCost,
    MoneyResource Total,
    string Periodicity = "Annual");
