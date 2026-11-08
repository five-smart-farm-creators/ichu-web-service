using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Services;
using static FiveSmartFarmCreators.Ichu.Platform.Tests.DashboardAnalytics.AnalyticsFixtures;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.DashboardAnalytics.Application;

public class AnalyticsQueryServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 15, 9, 0, 0, TimeSpan.Zero);
    private static readonly Guid OtherRanch = Guid.Parse("55555555-5555-4555-8555-555555555555");

    [Fact]
    public async Task HerdIndicators_RefusesACallerWithoutAVerifiedAccountBeforeReadingAnySource()
    {
        var source = new FakeAnalyticsSource { Snapshot = Snapshot() };
        var service = CreateService(new FakeAnalyticsAccess { AccountId = null }, source);

        var result = await service.HandleAsync(new GetHerdIndicatorsQuery(RanchId, Period));

        Assert.Equal(DashboardAnalyticsErrors.RanchAccessDenied.Code, result.DomainError.Code);
        Assert.Equal(0, source.SnapshotReads);
    }

    [Fact]
    public async Task HerdIndicators_RefusesACallerWithoutReadAccessBeforeReadingAnySource()
    {
        var source = new FakeAnalyticsSource { Snapshot = Snapshot() };
        var service = CreateService(new FakeAnalyticsAccess { CanRead = false }, source);

        var result = await service.HandleAsync(new GetHerdIndicatorsQuery(RanchId, Period));

        Assert.Equal(DashboardAnalyticsErrors.RanchAccessDenied.Code, result.DomainError.Code);
        Assert.Equal(0, source.SnapshotReads);
    }

    [Fact]
    public async Task HerdIndicators_RequiresARanchIdentifier()
    {
        var service = CreateService(new FakeAnalyticsAccess(), new FakeAnalyticsSource { Snapshot = Snapshot() });

        var result = await service.HandleAsync(new GetHerdIndicatorsQuery(Guid.Empty, Period));

        Assert.Equal(DashboardAnalyticsErrors.RanchRequired.Code, result.DomainError.Code);
    }

    [Fact]
    public async Task HerdIndicators_ReportsSourceUnavailableForASnapshotOfAnotherRanch()
    {
        var snapshot = Snapshot() with { RanchId = OtherRanch };
        var service = CreateService(new FakeAnalyticsAccess(), new FakeAnalyticsSource { Snapshot = snapshot });

        var result = await service.HandleAsync(new GetHerdIndicatorsQuery(RanchId, Period));

        Assert.Equal(DashboardAnalyticsErrors.SourceUnavailable.Code, result.DomainError.Code);
    }

    [Fact]
    public async Task HerdIndicators_ReportsSourceUnavailableForAReadingOfAnUnknownAnimal()
    {
        var snapshot = Snapshot(animals: [Animal(Guid.NewGuid())], readings: [Temperature(Guid.NewGuid(), Day(2), 38.5)]);
        var service = CreateService(new FakeAnalyticsAccess(), new FakeAnalyticsSource { Snapshot = snapshot });

        var result = await service.HandleAsync(new GetHerdIndicatorsQuery(RanchId, Period));

        Assert.Equal(DashboardAnalyticsErrors.SourceUnavailable.Code, result.DomainError.Code);
    }

    [Fact]
    public async Task HerdIndicators_AcceptsAPeriodThatEndsInTheFuture()
    {
        var snapshot = Snapshot(animals: [Animal(Guid.NewGuid())], updatedAt: Now);
        var service = CreateService(new FakeAnalyticsAccess(), new FakeAnalyticsSource { Snapshot = snapshot });

        var result = await service.HandleAsync(new GetHerdIndicatorsQuery(RanchId, Period));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.MortalityRate.Denominator);
    }

    [Fact]
    public async Task Portfolio_IsEmptyWhenTheAccountHasNoAccessibleRanch()
    {
        var service = CreateService(new FakeAnalyticsAccess(), new FakeAnalyticsSource());

        var result = await service.HandleAsync(new GetRanchPortfolioQuery());

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task Portfolio_RefusesAnUnverifiedCaller()
    {
        var service = CreateService(new FakeAnalyticsAccess { AccountId = null }, new FakeAnalyticsSource());

        var result = await service.HandleAsync(new GetRanchPortfolioQuery());

        Assert.Equal(DashboardAnalyticsErrors.RanchAccessDenied.Code, result.DomainError.Code);
    }

    [Fact]
    public async Task Portfolio_ReportsSourceUnavailableForAnEntryOfAnotherRanch()
    {
        var access = new FakeAnalyticsAccess { AccessibleRanches = [RanchId] };
        var source = new FakeAnalyticsSource { Portfolio = [new RanchPortfolioEntry(OtherRanch, Now, "ichu-platform", 0, 0, 0, null, null)] };
        var service = CreateService(access, source);

        var result = await service.HandleAsync(new GetRanchPortfolioQuery());

        Assert.Equal(DashboardAnalyticsErrors.SourceUnavailable.Code, result.DomainError.Code);
    }

    [Fact]
    public async Task Portfolio_OrdersTheRanchesByCriticalAlertsAndKeepsUnobservedCounts()
    {
        var quiet = new RanchPortfolioEntry(RanchId, Now, "ichu-platform", 1, 0, 0, null, null);
        var busy = new RanchPortfolioEntry(OtherRanch, Now, "ichu-platform", 3, 0, 0, null, null);
        var access = new FakeAnalyticsAccess { AccessibleRanches = [RanchId, OtherRanch] };
        var service = CreateService(access, new FakeAnalyticsSource { Portfolio = [quiet, busy] });

        var result = await service.HandleAsync(new GetRanchPortfolioQuery());

        Assert.Equal(new[] { OtherRanch, RanchId }, result.Value!.Select(entry => entry.RanchId));
    }

    private static AnalyticsQueryService CreateService(FakeAnalyticsAccess access, FakeAnalyticsSource source) =>
        new(access, source, new AnalyticsDomainService());
}
