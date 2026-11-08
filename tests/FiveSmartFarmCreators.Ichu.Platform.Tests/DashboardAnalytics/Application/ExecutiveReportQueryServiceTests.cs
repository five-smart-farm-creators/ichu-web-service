using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Services;
using Microsoft.Extensions.Time.Testing;
using static FiveSmartFarmCreators.Ichu.Platform.Tests.DashboardAnalytics.AnalyticsFixtures;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.DashboardAnalytics.Application;

public class ExecutiveReportQueryServiceTests
{
    private static readonly DateTimeOffset GeneratedAt = new(2026, 12, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly Guid OtherRanch = Guid.Parse("55555555-5555-4555-8555-555555555555");

    [Fact]
    public async Task HandleAsync_RejectsAnUnsupportedFormatBeforeReadingAnySource()
    {
        var source = new FakeAnalyticsSource { Snapshot = Snapshot(animals: [Animal(Guid.NewGuid())]) };
        var service = CreateService(new FakeAnalyticsAccess(), source, included: true);

        var result = await service.HandleAsync(new GetExecutiveReportQuery(RanchId, Period, "xml"));

        Assert.Equal(DashboardAnalyticsErrors.UnsupportedReportFormat.Code, result.DomainError.Code);
        Assert.Equal(0, source.SnapshotReads);
    }

    [Fact]
    public async Task HandleAsync_RefusesExportsThatTheSubscriptionDoesNotInclude()
    {
        var service = CreateService(new FakeAnalyticsAccess(),
            new FakeAnalyticsSource { Snapshot = Snapshot(animals: [Animal(Guid.NewGuid())]) }, included: false);

        var result = await service.HandleAsync(new GetExecutiveReportQuery(RanchId, Period, "json"));

        Assert.Equal(DashboardAnalyticsErrors.ExportNotIncluded.Code, result.DomainError.Code);
    }

    [Fact]
    public async Task HandleAsync_ReportsNoDataForAPeriodWithoutAnimals()
    {
        var service = CreateService(new FakeAnalyticsAccess(), new FakeAnalyticsSource { Snapshot = Snapshot() }, included: true);

        var result = await service.HandleAsync(new GetExecutiveReportQuery(RanchId, Period, "csv"));

        Assert.Equal(DashboardAnalyticsErrors.NoDataToExport.Code, result.DomainError.Code);
    }

    [Fact]
    public async Task HandleAsync_ExportsJsonNamedAfterTheRanchAndTheInstantOfGeneration()
    {
        var service = CreateService(new FakeAnalyticsAccess(),
            new FakeAnalyticsSource { Snapshot = Snapshot(animals: [Animal(Guid.NewGuid())]) }, included: true);

        var report = (await service.HandleAsync(new GetExecutiveReportQuery(RanchId, Period, "json"))).Value!;

        Assert.Equal("application/json", report.ContentType);
        Assert.Equal($"executive-report-{RanchId}-20261201100000.json", report.FileName);
    }

    [Fact]
    public async Task HandleAsync_RefusesACallerWithoutReadAccess()
    {
        var service = CreateService(new FakeAnalyticsAccess { CanRead = false },
            new FakeAnalyticsSource { Snapshot = Snapshot(animals: [Animal(Guid.NewGuid())]) }, included: true);

        var result = await service.HandleAsync(new GetExecutiveReportQuery(RanchId, Period, "json"));

        Assert.Equal(DashboardAnalyticsErrors.RanchAccessDenied.Code, result.DomainError.Code);
    }

    [Fact]
    public async Task HandleAsync_ReportsSourceUnavailableForAnInconsistentSnapshot()
    {
        var snapshot = Snapshot(animals: [Animal(Guid.NewGuid())]) with { RanchId = OtherRanch };
        var service = CreateService(new FakeAnalyticsAccess(), new FakeAnalyticsSource { Snapshot = snapshot }, included: true);

        var result = await service.HandleAsync(new GetExecutiveReportQuery(RanchId, Period, "json"));

        Assert.Equal(DashboardAnalyticsErrors.SourceUnavailable.Code, result.DomainError.Code);
    }

    private static ExecutiveReportQueryService CreateService(FakeAnalyticsAccess access, FakeAnalyticsSource source, bool included = true) =>
        new(new AnalyticsQueryService(access, source, new AnalyticsDomainService()), new FakeAnalyticsEntitlements(included),
            new FakeTimeProvider(GeneratedAt));
}
