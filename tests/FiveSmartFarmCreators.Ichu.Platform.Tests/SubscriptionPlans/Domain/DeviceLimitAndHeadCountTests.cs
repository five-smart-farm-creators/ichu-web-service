using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.SubscriptionPlans.Domain;

public class DeviceLimitAndHeadCountTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DeviceLimit_RejectsLimitsBelowOne(int maximum)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DeviceLimit(maximum));
    }

    [Fact]
    public void DeviceLimit_AllowsDevicesOnlyWhileUsageIsBelowTheMaximum()
    {
        var limit = new DeviceLimit(50);

        Assert.True(limit.Allows(49));
        Assert.False(limit.Allows(50));
    }

    [Fact]
    public void DeviceLimit_CoversHeadCountsUpToAndIncludingTheMaximum()
    {
        var limit = new DeviceLimit(50);

        Assert.True(limit.Covers(50));
        Assert.False(limit.Covers(51));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10_000)]
    public void HeadCount_CreateAcceptsTheEdgesOfTheRange(int value)
    {
        var headCount = HeadCount.Create(value);

        Assert.True(headCount.IsSuccess);
        Assert.Equal(value, headCount.Value!.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10_001)]
    public void HeadCount_CreateReturnsInvalidHeadCountOutsideTheRange(int value)
    {
        var headCount = HeadCount.Create(value);

        Assert.True(headCount.IsFailure);
        Assert.Equal(SubscriptionPlansErrors.InvalidHeadCount.Code, headCount.DomainError.Code);
    }

    [Fact]
    public void HeadCount_ConstructorRejectsValuesOutsideTheRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HeadCount(0));
    }
}
