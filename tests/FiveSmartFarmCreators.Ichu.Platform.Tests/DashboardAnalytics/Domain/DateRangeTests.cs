using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.DashboardAnalytics.Domain;

public class DateRangeTests
{
    private static readonly DateTimeOffset Start = new(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_RejectsAWindowThatDoesNotEndAfterItStarts(int daysToEnd)
    {
        var result = DateRange.Create(Start, Start.AddDays(daysToEnd));

        Assert.True(result.IsFailure);
        Assert.Equal(DashboardAnalyticsErrors.InvalidPeriod.Code, result.DomainError.Code);
    }

    [Fact]
    public void Create_NormalizesBoundariesToUtc()
    {
        var local = new DateTimeOffset(2026, 11, 1, 2, 0, 0, TimeSpan.FromHours(2));

        var range = DateRange.Create(local, local.AddDays(1)).Value!;

        Assert.Equal(TimeSpan.Zero, range.From.Offset);
        Assert.Equal(Start, range.From);
    }

    [Fact]
    public void Contains_IncludesTheStartAndExcludesTheEnd()
    {
        var range = DateRange.Create(Start, Start.AddDays(1)).Value!;

        Assert.True(range.Contains(Start));
        Assert.False(range.Contains(Start.AddDays(1)));
    }
}
