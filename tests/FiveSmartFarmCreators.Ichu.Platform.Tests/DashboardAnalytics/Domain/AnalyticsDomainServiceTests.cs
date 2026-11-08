using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Services;
using static FiveSmartFarmCreators.Ichu.Platform.Tests.DashboardAnalytics.AnalyticsFixtures;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.DashboardAnalytics.Domain;

public class AnalyticsDomainServiceTests
{
    private static readonly Guid Calf = Guid.Parse("33333333-3333-4333-8333-333333333331");
    private static readonly Guid Heifer = Guid.Parse("33333333-3333-4333-8333-333333333332");
    private static readonly Guid Cow = Guid.Parse("33333333-3333-4333-8333-333333333333");

    private readonly AnalyticsDomainService domain = new();

    [Fact]
    public void BuildIndicators_CountsTheAnimalsWithValidReadingsInsideThePeriod()
    {
        var snapshot = Snapshot(
            animals: [Animal(Calf), Animal(Heifer), Animal(Cow)],
            readings:
            [
                Temperature(Calf, Day(2), 38.5),
                new ReadingSnapshot(Heifer, Day(3), null, double.NaN, null),
                Temperature(Cow, PeriodEnd.AddDays(1), 38.2)
            ]);

        var indicators = domain.BuildIndicators(snapshot);

        Assert.Equal(1, indicators.MonitoredAnimals.Value);
        Assert.Equal(3, indicators.MonitoredAnimals.Denominator);
    }

    [Fact]
    public void BuildIndicators_ComputesMortalityAsDeathsOverTheAnimalsAtRisk()
    {
        var snapshot = Snapshot(animals: [Animal(Calf, died: true), Animal(Heifer), Animal(Cow), Animal(Guid.NewGuid())]);

        var indicators = domain.BuildIndicators(snapshot);

        Assert.Equal(25.0, indicators.MortalityRate.Value);
        Assert.Equal(4, indicators.MortalityRate.Denominator);
    }

    [Fact]
    public void BuildIndicators_CountsOpenAlertsByTypeAtThePeriodEnd()
    {
        var snapshot = Snapshot(
            animals: [Animal(Calf)],
            alerts:
            [
                new AlertSnapshot(Guid.NewGuid(), Calf, "temperature", Day(2), null),
                new AlertSnapshot(Guid.NewGuid(), Calf, "temperature", Day(3), Day(10)),
                new AlertSnapshot(Guid.NewGuid(), Calf, "geofence", Day(5), PeriodEnd)
            ]);

        var indicators = domain.BuildIndicators(snapshot);

        Assert.NotNull(indicators.OpenAlertsByType);
        Assert.Equal(2, indicators.OpenAlertsByType.Count);
        Assert.Equal(1, indicators.OpenAlertsByType["temperature"]);
        Assert.Equal(1, indicators.OpenAlertsByType["geofence"]);
    }

    [Fact]
    public void BuildIndicators_ReportsUnavailableValuesWhenThePopulationIsIncomplete()
    {
        var snapshot = Snapshot(animals: [Animal(Calf, died: true)], populationComplete: false);

        var indicators = domain.BuildIndicators(snapshot);

        Assert.Null(indicators.MonitoredAnimals.Value);
        Assert.Equal("The population snapshot is incomplete.", indicators.MonitoredAnimals.UnavailableReason);
        Assert.Null(indicators.MortalityRate.Value);
        Assert.Null(indicators.OutsideGeofenceAnimals.Value);
    }

    [Fact]
    public void BuildTrend_WeightsEachAnimalEquallyWhateverItsReadingCount()
    {
        var snapshot = Snapshot(
            animals: [Animal(Calf), Animal(Heifer)],
            readings:
            [
                Temperature(Calf, Day(2), 30),
                Temperature(Calf, Day(3), 30),
                Temperature(Calf, Day(4), 30),
                Temperature(Heifer, Day(2), 40)
            ]);

        var trend = domain.BuildTrend(snapshot, LotId, "body-temperature", 5).Value!;

        Assert.Equal(35, trend.LotAverage);
        Assert.Equal("celsius", trend.Unit);
        Assert.Equal(4, trend.Points.Count);
    }

    [Fact]
    public void BuildTrend_FlagsTheAnimalsWhoseAverageDeviatesBeyondTheThreshold()
    {
        var snapshot = Snapshot(
            animals: [Animal(Calf), Animal(Heifer), Animal(Cow)],
            readings: [Temperature(Calf, Day(2), 30), Temperature(Heifer, Day(2), 31), Temperature(Cow, Day(2), 40)]);

        var trend = domain.BuildTrend(snapshot, LotId, "body-temperature", 5).Value!;

        Assert.Equal(Cow, Assert.Single(trend.OutlierAnimals));
    }

    [Fact]
    public void BuildTrend_RejectsAnUnsupportedMetric()
    {
        var result = domain.BuildTrend(Snapshot(), LotId, "heart-rate", 5);

        Assert.Equal(DashboardAnalyticsErrors.UnsupportedMetric.Code, result.DomainError.Code);
    }

    [Fact]
    public void BuildTrend_RequiresALot()
    {
        var result = domain.BuildTrend(Snapshot(), Guid.Empty, "body-temperature", 5);

        Assert.Equal(DashboardAnalyticsErrors.LotRequired.Code, result.DomainError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    public void BuildTrend_RejectsANonPositiveOrNonFiniteOutlierDeviation(double deviation)
    {
        var result = domain.BuildTrend(Snapshot(), LotId, "body-temperature", deviation);

        Assert.Equal(DashboardAnalyticsErrors.InvalidOutlierDeviation.Code, result.DomainError.Code);
    }

    [Fact]
    public void CompareAgainstLot_ReportsUnavailableWithFewerThanFiveAnimals()
    {
        var snapshot = Snapshot(
            animals: [Animal(Calf), Animal(Heifer)],
            readings: [Temperature(Calf, Day(2), 38), Temperature(Heifer, Day(2), 39)]);
        var trend = domain.BuildTrend(snapshot, LotId, "body-temperature", 5).Value!;

        var comparison = domain.CompareAgainstLot(trend, Calf);

        Assert.Null(comparison.AnimalAverage);
        Assert.Equal(2, comparison.ReferenceAnimalCount);
        Assert.Equal("At least five animals with valid telemetry are required.", comparison.UnavailableReason);
    }

    [Fact]
    public void BuildCurve_GroupsCasesByOnsetDayAndCountsTheDaysWithoutNewCases()
    {
        var snapshot = Snapshot(
            cases:
            [
                new HealthCaseSnapshot(Guid.NewGuid(), Calf, "temperature", Day(2)),
                new HealthCaseSnapshot(Guid.NewGuid(), Heifer, "temperature", Day(3))
            ]);

        var curve = domain.BuildCurve(snapshot, "temperature").Value!;

        Assert.Equal(2, curve.NewCasesByDay.Values.Sum());
        Assert.Equal(28, curve.DaysWithoutNewCases);
        Assert.True(curve.SupportsRestrictionLifting);
        Assert.Null(curve.UnavailableReason);
    }

    [Fact]
    public void BuildCurve_ReportsUnavailableWithFewerThanTwoCases()
    {
        var snapshot = Snapshot(cases: [new HealthCaseSnapshot(Guid.NewGuid(), Calf, "temperature", Day(2))]);

        var curve = domain.BuildCurve(snapshot, "temperature").Value!;

        Assert.Empty(curve.NewCasesByDay);
        Assert.Equal("At least two recorded cases are required to build a progression.", curve.UnavailableReason);
    }

    [Fact]
    public void BuildCurve_ReportsUnavailableForAConditionWithoutIncubationPeriod()
    {
        var snapshot = Snapshot(
            cases:
            [
                new HealthCaseSnapshot(Guid.NewGuid(), Calf, "brucellosis", Day(2)),
                new HealthCaseSnapshot(Guid.NewGuid(), Heifer, "brucellosis", Day(3))
            ]);

        var curve = domain.BuildCurve(snapshot, "brucellosis").Value!;

        Assert.Equal("The condition has no configured incubation period.", curve.UnavailableReason);
    }

    [Fact]
    public void BuildCurve_ReportsAnObservationShorterThanTheIncubationPeriod()
    {
        var threeDays = DateRange.Create(Day(1), Day(4)).Value!;
        var snapshot = Snapshot(
            period: threeDays,
            cases:
            [
                new HealthCaseSnapshot(Guid.NewGuid(), Calf, "temperature", Day(2)),
                new HealthCaseSnapshot(Guid.NewGuid(), Heifer, "temperature", Day(3))
            ]);

        var curve = domain.BuildCurve(snapshot, "temperature").Value!;

        Assert.Null(curve.SupportsRestrictionLifting);
        Assert.Equal("The observation period is shorter than the incubation period.", curve.UnavailableReason);
    }

    [Fact]
    public void BuildCurve_RequiresACondition()
    {
        var result = domain.BuildCurve(Snapshot(), " ");

        Assert.Equal(DashboardAnalyticsErrors.ConditionRequired.Code, result.DomainError.Code);
    }
}
