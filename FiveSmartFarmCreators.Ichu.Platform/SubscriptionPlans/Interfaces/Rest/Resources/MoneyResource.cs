using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;

public sealed record MoneyResource(decimal Amount, string Currency)
{
    public static MoneyResource From(Money money) => new(money.Amount, money.Currency);
}
