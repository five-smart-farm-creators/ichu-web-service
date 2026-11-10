using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.SubscriptionPlans.Domain;

public class MoneyTests
{
    [Fact]
    public void Constructor_RoundsAmountToCentsAwayFromZero()
    {
        var money = new Money(10.005m, "PEN");

        Assert.Equal(10.01m, money.Amount);
    }

    [Fact]
    public void Constructor_RejectsNegativeAmount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Money(-0.01m, "PEN"));
    }

    [Theory]
    [InlineData("pen")]
    [InlineData("PE")]
    [InlineData("PEN1")]
    public void Constructor_RejectsCurrencyThatIsNotAThreeLetterUpperCaseCode(string currency)
    {
        Assert.Throws<ArgumentException>(() => new Money(1m, currency));
    }

    [Fact]
    public void Add_SumsAmountsInTheSameCurrency()
    {
        var total = new Money(600m, "PEN").Add(new Money(120m, "PEN"));

        Assert.Equal(720m, total.Amount);
        Assert.Equal("PEN", total.Currency);
    }

    [Fact]
    public void Add_RejectsAmountsInDifferentCurrencies()
    {
        Assert.Throws<ArgumentException>(() => new Money(600m, "PEN").Add(new Money(100m, "USD")));
    }

    [Fact]
    public void Multiply_ScalesTheAmountByTheNumberOfUnits()
    {
        var devicesCost = new Money(120m, "PEN").Multiply(120);

        Assert.Equal(14400m, devicesCost.Amount);
    }

    [Fact]
    public void ToMinorUnits_ConvertsTheAmountToHundredths()
    {
        Assert.Equal(300000L, new Money(3000m, "PEN").ToMinorUnits());
    }

    [Fact]
    public void ToMinorUnits_UsesTheAmountRoundedAwayFromZero()
    {
        Assert.Equal(1001L, new Money(10.005m, "PEN").ToMinorUnits());
    }
}
