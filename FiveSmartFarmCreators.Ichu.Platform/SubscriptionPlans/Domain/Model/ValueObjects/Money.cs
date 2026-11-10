namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

/// <summary>
///     Monetary amount in one currency. Amounts are rounded to cents so that quotes add up exactly.
/// </summary>
public record Money
{
    /// <summary>
    ///     Creates an amount. Negative amounts and malformed currency codes are programming errors, not user input.
    /// </summary>
    public Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "A monetary amount cannot be negative.");
        }

        if (currency is not { Length: 3 } || !currency.All(char.IsAsciiLetterUpper))
        {
            throw new ArgumentException("The currency must be a three-letter upper-case ISO code.", nameof(currency));
        }

        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency;
    }

    public decimal Amount { get; }

    public string Currency { get; }

    /// <summary>
    ///     Adds an amount of the same currency. Mixing currencies would produce a total that means nothing.
    /// </summary>
    public Money Add(Money other)
    {
        if (other.Currency != Currency)
        {
            throw new ArgumentException("Amounts in different currencies cannot be added.", nameof(other));
        }

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Multiply(int units) => new(Amount * units, Currency);

    /// <summary>
    ///     Converts the amount to minor currency units, such as cents, which payment providers expect as integers.
    /// </summary>
    public long ToMinorUnits() => (long)decimal.Round(Amount * 100m, 0, MidpointRounding.AwayFromZero);
}
