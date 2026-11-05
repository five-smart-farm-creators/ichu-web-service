namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

/// <summary>A non-negative amount with at most two decimals in an ISO 4217 currency.</summary>
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0 || decimal.Round(amount, 2) != amount)
        {
            throw SubscriptionException.InvalidInput("An amount must be non-negative with at most two decimals.");
        }

        var code = currency.Trim().ToUpperInvariant();
        if (code.Length != 3 || !code.All(char.IsAsciiLetterUpper))
        {
            throw SubscriptionException.InvalidInput("A currency must be a three-letter ISO 4217 code.");
        }

        Amount = amount;
        Currency = code;
    }

    public Money Multiply(int factor)
    {
        if (factor < 0)
        {
            throw SubscriptionException.InvalidInput("An amount cannot be multiplied by a negative factor.");
        }

        return new Money(Amount * factor, Currency);
    }
}
