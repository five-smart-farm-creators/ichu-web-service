using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Services;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.OperationsMonitoring.Domain;

public class ReproductiveHistoryCalculatorTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid CattleId = Guid.Parse("7b2e3d1f-1e4c-4d62-8b88-2c3d4e5f6a71");
    private static readonly Guid AuthorId = Guid.Parse("44444444-4444-4444-8444-444444444444");
    private readonly ReproductiveHistoryCalculator _calculator = new();

    [Fact]
    public void DaysOpenRunFromThePreviousCalvingToTheConceptionService()
    {
        var history = _calculator.Calculate(AnimalSex.Female, [
            Operation(OperationType.Birth, new DateOnly(2026, 1, 1)),
            Operation(OperationType.Insemination, new DateOnly(2026, 2, 20), sire: "Bull 07"),
            Operation(OperationType.PregnancyDiagnosis, new DateOnly(2026, 3, 10), result: "Pregnant")
        ]);

        var interval = Assert.Single(history.Intervals);
        Assert.Equal(Day(2026, 1, 1), interval.CalvingAt);
        Assert.Equal(Day(2026, 2, 20), interval.ConceptionServiceAt);
        Assert.Equal(50, interval.DaysOpen);
        Assert.Equal(1, history.PregnanciesConfirmed);
        Assert.Equal(1.0, history.ServicesPerPregnancy);
    }

    [Fact]
    public void RechecksOfOneGestationCountOnce()
    {
        var history = _calculator.Calculate(AnimalSex.Female, [
            Operation(OperationType.Insemination, new DateOnly(2026, 1, 10)),
            Operation(OperationType.PregnancyDiagnosis, new DateOnly(2026, 2, 1), result: "Pregnant"),
            Operation(OperationType.PregnancyDiagnosis, new DateOnly(2026, 3, 1), result: "Pregnant"),
            Operation(OperationType.Birth, new DateOnly(2026, 10, 1))
        ]);

        Assert.Equal(1, history.PregnanciesConfirmed);
        Assert.Equal(2, history.PregnancyDiagnoses.Count);
        Assert.Equal(1.0, history.ServicesPerPregnancy);
    }

    [Fact]
    public void AbortionEndsTheWindowSoTheServiceAfterItConceives()
    {
        var history = _calculator.Calculate(AnimalSex.Female, [
            Operation(OperationType.Birth, new DateOnly(2026, 1, 1)),
            Operation(OperationType.Insemination, new DateOnly(2026, 1, 10), sire: "Bull 07"),
            Operation(OperationType.PregnancyDiagnosis, new DateOnly(2026, 2, 1), result: "Pregnant"),
            Operation(OperationType.Abortion, new DateOnly(2026, 2, 20)),
            Operation(OperationType.Insemination, new DateOnly(2026, 3, 10), sire: "Bull 07"),
            Operation(OperationType.PregnancyDiagnosis, new DateOnly(2026, 4, 1), result: "Pregnant")
        ]);

        Assert.Equal(2, history.PregnanciesConfirmed);
        Assert.Equal(1.0, history.ServicesPerPregnancy);
        var interval = Assert.Single(history.Intervals);
        Assert.Equal(Day(2026, 3, 10), interval.ConceptionServiceAt);
        Assert.Equal(68, interval.DaysOpen);
    }

    [Fact]
    public void CalvingWithoutAFollowingConceptionIsOpen()
    {
        var history = _calculator.Calculate(AnimalSex.Female, [
            Operation(OperationType.Birth, new DateOnly(2026, 1, 1)),
            Operation(OperationType.HeatObserved, new DateOnly(2026, 1, 20))
        ]);

        var interval = Assert.Single(history.Intervals);
        Assert.Equal(Day(2026, 1, 1), interval.CalvingAt);
        Assert.Null(interval.ConceptionServiceAt);
        Assert.Null(interval.DaysOpen);
        Assert.True(history.HasHistory);
        Assert.Null(history.ServicesPerPregnancy);
        Assert.Equal(0, history.PregnanciesConfirmed);
    }

    [Fact]
    public void HeatAloneIsNotAHistory()
    {
        var history = _calculator.Calculate(AnimalSex.Female, [Operation(OperationType.HeatObserved, new DateOnly(2026, 1, 5))]);

        Assert.False(history.HasHistory);
        Assert.Single(history.HeatsObserved);
    }

    [Fact]
    public void FemaleWithoutServiceOrCalvingHasNoHistory()
    {
        var history = _calculator.Calculate(AnimalSex.Female, []);

        Assert.False(history.HasHistory);
        Assert.Empty(history.Intervals);
        Assert.Equal(0, history.PregnanciesConfirmed);
        Assert.Null(history.ServicesPerPregnancy);
    }

    [Fact]
    public void MaleHasNoHistoryWhateverItsOperations()
    {
        var history = _calculator.Calculate(AnimalSex.Male, [
            Operation(OperationType.Birth, new DateOnly(2026, 1, 1)),
            Operation(OperationType.Insemination, new DateOnly(2026, 2, 1), sire: "Bull 07")
        ]);

        Assert.False(history.HasHistory);
        Assert.Empty(history.Services);
        Assert.Empty(history.Calvings);
        Assert.Empty(history.Intervals);
    }

    [Fact]
    public void ServicesPerPregnancyDividesEveryBreedingServiceByTheConfirmedPregnancies()
    {
        var history = _calculator.Calculate(AnimalSex.Female, [
            Operation(OperationType.Birth, new DateOnly(2025, 1, 1)),
            Operation(OperationType.Insemination, new DateOnly(2025, 2, 20), sire: "Bull 07"),
            Operation(OperationType.PregnancyDiagnosis, new DateOnly(2025, 3, 10), result: "Pregnant"),
            Operation(OperationType.PregnancyDiagnosis, new DateOnly(2025, 4, 10), result: "Pregnant"),
            Operation(OperationType.HeatObserved, new DateOnly(2025, 9, 20)),
            Operation(OperationType.Birth, new DateOnly(2025, 10, 8)),
            Operation(OperationType.NaturalMating, new DateOnly(2025, 10, 20), sire: "Bull 12"),
            Operation(OperationType.Insemination, new DateOnly(2025, 11, 20), sire: "Bull 07"),
            Operation(OperationType.PregnancyDiagnosis, new DateOnly(2025, 12, 10), result: "Pregnant")
        ]);

        Assert.Equal(2, history.PregnanciesConfirmed);
        Assert.Equal(1.5, history.ServicesPerPregnancy);
        Assert.Collection(history.Intervals,
            first =>
            {
                Assert.Equal(Day(2025, 1, 1), first.CalvingAt);
                Assert.Equal(Day(2025, 2, 20), first.ConceptionServiceAt);
                Assert.Equal(50, first.DaysOpen);
            },
            second =>
            {
                Assert.Equal(Day(2025, 10, 8), second.CalvingAt);
                Assert.Equal(Day(2025, 11, 20), second.ConceptionServiceAt);
                Assert.Equal(43, second.DaysOpen);
            });
    }

    private static DateTimeOffset Day(int year, int month, int day) =>
        new(year, month, day, 8, 0, 0, TimeSpan.Zero);

    private static LivestockOperation Operation(OperationType type, DateOnly day, string? result = null, string? sire = null)
    {
        var at = new DateTimeOffset(day.ToDateTime(new TimeOnly(8, 0)), TimeSpan.Zero);
        var details = ReproductiveDetails.Create(sire, result, null).Value!;
        return LivestockOperation.Create(Guid.NewGuid(), RanchId, CattleId, type, at, AuthorId, null, null, null, at, details).Value!;
    }
}
