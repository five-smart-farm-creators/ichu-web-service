using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using static FiveSmartFarmCreators.Ichu.Platform.Tests.DashboardAnalytics.AnalyticsFixtures;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.DashboardAnalytics.Application;

public class PlatformAnalyticsSourceTests
{
    private static readonly Guid Alive = Guid.Parse("33333333-3333-4333-8333-333333333331");
    private static readonly Guid Dead = Guid.Parse("33333333-3333-4333-8333-333333333332");
    private static readonly Guid Sold = Guid.Parse("33333333-3333-4333-8333-333333333333");
    private static readonly Guid Outside = Guid.Parse("33333333-3333-4333-8333-333333333334");
    private static readonly Guid Returned = Guid.Parse("33333333-3333-4333-8333-333333333335");
    private static readonly DateTimeOffset Now = new(2026, 12, 2, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly PeriodStartDate = new(2026, 11, 1);
    private static readonly DateOnly PeriodEndDate = new(2026, 12, 1);

    [Fact]
    public async Task ReadAsync_PopulationExcludesAnimalsBornAtOrAfterThePeriodEnd()
    {
        var newborn = Guid.NewGuid();
        var source = CreateSource(new FakeCattleInformation(Member(Alive, new DateOnly(2026, 10, 1)), Member(newborn, PeriodEndDate)),
            new FakeOperationsMonitoring());

        var snapshot = await source.ReadAsync(RanchId, Period, CancellationToken.None);

        Assert.Equal(Alive, Assert.Single(snapshot.Animals).AnimalId);
    }

    [Fact]
    public async Task ReadAsync_PopulationExcludesAnimalsThatExitedBeforeThePeriodStart()
    {
        var exitedEarlier = Member(Sold, new DateOnly(2026, 1, 1), "Sale", new DateOnly(2026, 10, 31));
        var exitedOnStart = Member(Alive, new DateOnly(2026, 1, 1), "Sale", PeriodStartDate);
        var source = CreateSource(new FakeCattleInformation(exitedEarlier, exitedOnStart), new FakeOperationsMonitoring());

        var snapshot = await source.ReadAsync(RanchId, Period, CancellationToken.None);

        Assert.Equal(Alive, Assert.Single(snapshot.Animals).AnimalId);
    }

    [Fact]
    public async Task ReadAsync_CountsADeathOnlyWhenItsExitDateFallsInsideThePeriod()
    {
        var death = Member(Dead, new DateOnly(2026, 1, 1), "Death", new DateOnly(2026, 11, 10));
        var sale = Member(Sold, new DateOnly(2026, 1, 1), "Sale", new DateOnly(2026, 11, 10));
        var deathOnPeriodEnd = Member(Alive, new DateOnly(2026, 1, 1), "Death", PeriodEndDate);
        var source = CreateSource(new FakeCattleInformation(death, sale, deathOnPeriodEnd), new FakeOperationsMonitoring());

        var snapshot = await source.ReadAsync(RanchId, Period, CancellationToken.None);

        Assert.True(snapshot.Animals.Single(a => a.AnimalId == Dead).DiedInPeriod);
        Assert.False(snapshot.Animals.Single(a => a.AnimalId == Sold).DiedInPeriod);
        Assert.False(snapshot.Animals.Single(a => a.AnimalId == Alive).DiedInPeriod);
    }

    [Fact]
    public async Task ReadAsync_MarksAnAnimalOutsideTheGeofenceOnlyWhileItsAlertIsOpenAtThePeriodEnd()
    {
        var operations = new FakeOperationsMonitoring();
        operations.Alerts.Add(Alert(Outside, AlertType.Geofence, Day(5), cleared: null));
        operations.Alerts.Add(Alert(Returned, AlertType.Geofence, Day(5), cleared: Day(20)));
        operations.Alerts.Add(Alert(Alive, AlertType.Geofence, Day(5), cleared: PeriodEnd));
        var source = CreateSource(new FakeCattleInformation(Member(Outside), Member(Returned), Member(Alive)), operations);

        var snapshot = await source.ReadAsync(RanchId, Period, CancellationToken.None);

        Assert.True(snapshot.Animals.Single(a => a.AnimalId == Outside).OutsideGeofence);
        Assert.False(snapshot.Animals.Single(a => a.AnimalId == Returned).OutsideGeofence);
        Assert.True(snapshot.Animals.Single(a => a.AnimalId == Alive).OutsideGeofence);
    }

    [Fact]
    public async Task ReadAsync_KeepsOnlyTheFirstReadingOfEachAnimalAndInstant()
    {
        var operations = new FakeOperationsMonitoring();
        operations.Readings.Add(Reading(Alive, Day(2), temperature: 38.0));
        operations.Readings.Add(Reading(Alive, Day(2), temperature: 39.0));
        var source = CreateSource(new FakeCattleInformation(Member(Alive)), operations);

        var snapshot = await source.ReadAsync(RanchId, Period, CancellationToken.None);

        Assert.Equal(38.0, Assert.Single(snapshot.Readings).BodyTemperature);
    }

    [Fact]
    public async Task ReadAsync_DropsReadingsOutsideThePeriodAndOfAnimalsOutsideThePopulation()
    {
        var operations = new FakeOperationsMonitoring();
        operations.Readings.Add(Reading(Alive, Day(1).AddMinutes(-1)));
        operations.Readings.Add(Reading(Alive, PeriodEnd));
        operations.Readings.Add(Reading(Guid.NewGuid(), Day(3)));
        operations.Readings.Add(Reading(Alive, Day(3)));
        var source = CreateSource(new FakeCattleInformation(Member(Alive)), operations);

        var snapshot = await source.ReadAsync(RanchId, Period, CancellationToken.None);

        Assert.Equal(Day(3), Assert.Single(snapshot.Readings).RecordedAt);
    }

    [Fact]
    public async Task ReadAsync_TurnsThermalAndActivityAlertsIntoCasesButNotGeofenceAlerts()
    {
        var thermal = Guid.NewGuid();
        var activity = Guid.NewGuid();
        var operations = new FakeOperationsMonitoring();
        operations.Alerts.Add(new MonitoringAlertRecord(thermal, Alive, AlertType.Temperature, AlertSeverity.Warning, Day(3), null, null));
        operations.Alerts.Add(new MonitoringAlertRecord(activity, Alive, AlertType.Activity, AlertSeverity.Informative, Day(4), null, null));
        operations.Alerts.Add(Alert(Alive, AlertType.Geofence, Day(5), cleared: null));
        var source = CreateSource(new FakeCattleInformation(Member(Alive)), operations);

        var snapshot = await source.ReadAsync(RanchId, Period, CancellationToken.None);

        Assert.Equal(2, snapshot.Cases.Count);
        Assert.Contains(snapshot.Cases, c => c.CaseId == thermal && c.Condition == "temperature" && c.Onset == Day(3));
        Assert.Contains(snapshot.Cases, c => c.CaseId == activity && c.Condition == "activity" && c.Onset == Day(4));
        Assert.Equal(3, snapshot.Alerts.Count);
    }

    [Fact]
    public async Task ReadAsync_ClosesAnAlertAtItsAcknowledgementOrClearanceWhicheverComesFirst()
    {
        var acknowledged = Guid.NewGuid();
        var cleared = Guid.NewGuid();
        var operations = new FakeOperationsMonitoring();
        operations.Alerts.Add(new MonitoringAlertRecord(acknowledged, Alive, AlertType.Temperature, AlertSeverity.Warning, Day(3), Day(4), null));
        operations.Alerts.Add(new MonitoringAlertRecord(cleared, Alive, AlertType.Geofence, AlertSeverity.Critical, Day(5), Day(7), Day(6)));
        var source = CreateSource(new FakeCattleInformation(Member(Alive)), operations);

        var snapshot = await source.ReadAsync(RanchId, Period, CancellationToken.None);

        Assert.Equal(Day(4), snapshot.Alerts.Single(a => a.AlertId == acknowledged).ClosedAt);
        Assert.Equal(Day(6), snapshot.Alerts.Single(a => a.AlertId == cleared).ClosedAt);
    }

    [Fact]
    public async Task ReadAsync_CountsOnlyTheBirthOperationsOfThePeriod()
    {
        var operations = new FakeOperationsMonitoring();
        operations.Operations.Add(Operation(OperationType.Birth, Day(3)));
        operations.Operations.Add(Operation(OperationType.Birth, Day(4)));
        operations.Operations.Add(Operation(OperationType.Treatment, Day(5)));
        var source = CreateSource(new FakeCattleInformation(Member(Alive)), operations);

        var snapshot = await source.ReadAsync(RanchId, Period, CancellationToken.None);

        Assert.Equal(2, snapshot.BirthCount);
    }

    [Fact]
    public async Task ReadAsync_AssignsAnimalsWithoutALotToTheEmptyLot()
    {
        var unassigned = new HerdMemberSnapshot(Alive, null, new DateOnly(2026, 1, 1), null, null);
        var source = CreateSource(new FakeCattleInformation(unassigned), new FakeOperationsMonitoring());

        var snapshot = await source.ReadAsync(RanchId, Period, CancellationToken.None);

        Assert.Equal(Guid.Empty, Assert.Single(snapshot.Animals).LotId);
    }

    [Fact]
    public async Task ReadAsync_CarriesTheConfiguredIncubationAndTheClockAndLeavesPregnanciesUnobserved()
    {
        var source = CreateSource(new FakeCattleInformation(), new FakeOperationsMonitoring());

        var snapshot = await source.ReadAsync(RanchId, Period, CancellationToken.None);

        Assert.Equal(7, snapshot.ConditionIncubationDays["temperature"]);
        Assert.Equal(Now, snapshot.UpdatedAt);
        Assert.Equal("ichu-platform", snapshot.Source);
        Assert.Null(snapshot.ConfirmedPregnancies);
        Assert.Null(snapshot.ExpectedReadings);
    }

    [Fact]
    public async Task ReadPortfolioAsync_ReportsTheOpenAlertCountsAndLeavesTheUnobservedSignalsUnavailable()
    {
        var operations = new FakeOperationsMonitoring { Counts = new OpenAlertCounts(2, 1, 0) };
        var source = CreateSource(new FakeCattleInformation(), operations);

        var entry = Assert.Single(await source.ReadPortfolioAsync([RanchId], CancellationToken.None));

        Assert.Equal(RanchId, entry.RanchId);
        Assert.Equal(2, entry.CriticalAlerts);
        Assert.Equal(1, entry.WarningAlerts);
        Assert.Equal(0, entry.InformativeAlerts);
        Assert.Null(entry.UnresponsiveTreatmentAnimals);
        Assert.Null(entry.OverdueCampaigns);
        Assert.Equal(Now, entry.UpdatedAt);
    }

    private static PlatformAnalyticsSource CreateSource(FakeCattleInformation cattle, FakeOperationsMonitoring operations) =>
        new(cattle, operations,
            Options.Create(new DashboardAnalyticsOptions { ConditionIncubationDays = new(StringComparer.OrdinalIgnoreCase) { ["temperature"] = 7 } }),
            new FakeTimeProvider(Now));

    private static HerdMemberSnapshot Member(Guid cattleId, DateOnly? birthDate = null, string? exitReason = null, DateOnly? exitDate = null, Guid? lotId = null) =>
        new(cattleId, lotId ?? LotId, birthDate ?? new DateOnly(2026, 1, 1), exitReason, exitDate);

    private static MonitoringReadingRecord Reading(Guid cattleId, DateTimeOffset recordedAt, double? temperature = 38.5) =>
        new(Guid.NewGuid(), Guid.NewGuid(), cattleId, recordedAt, temperature, null, null, null, null, null);

    private static MonitoringAlertRecord Alert(Guid cattleId, AlertType type, DateTimeOffset raisedAt, DateTimeOffset? cleared) =>
        new(Guid.NewGuid(), cattleId, type, AlertSeverity.Critical, raisedAt, null, cleared);

    private static LivestockOperationRecord Operation(OperationType type, DateTimeOffset occurredAt) =>
        new(Guid.NewGuid(), RanchId, Alive, type, type is OperationType.Treatment, occurredAt, Guid.NewGuid(), null, null, null);

    private sealed class FakeCattleInformation(params HerdMemberSnapshot[] herd) : ICattleInformationContextFacade
    {
        public Task<IReadOnlyList<HerdMemberSnapshot>> GetHerdAsync(Guid ranchId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<HerdMemberSnapshot>>(herd);

        public Task<CattleSnapshot?> FindByIdAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The analytics source reads the herd only.");

        public Task<bool> LotExistsAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The analytics source reads the herd only.");

        public Task<IReadOnlyList<CattleSnapshot>> FindActiveByLotAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The analytics source reads the herd only.");

        public Task<CattleLifecycleSnapshot?> FindLifecycleAsync(Guid cattleId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The analytics source reads the herd only.");

        public Task<IReadOnlyList<CattlePlacementSnapshot>> FindActivePlacementsAsync(Guid ranchId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The analytics source reads the herd only.");
    }

    private sealed class FakeOperationsMonitoring : IOperationsMonitoringContextFacade
    {
        public List<MonitoringReadingRecord> Readings { get; } = [];

        public List<MonitoringAlertRecord> Alerts { get; } = [];

        public List<LivestockOperationRecord> Operations { get; } = [];

        public OpenAlertCounts Counts { get; init; } = new(0, 0, 0);

        public Task<IReadOnlyList<MonitoringReadingRecord>> GetReadingsAsync(Guid ranchId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<MonitoringReadingRecord>>(Readings);

        public Task<IReadOnlyList<MonitoringAlertRecord>> GetAlertsAsync(Guid ranchId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<MonitoringAlertRecord>>(Alerts);

        public Task<DeviceCommunicationRecord?> GetLastDeviceCommunicationAsync(Guid deviceId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The analytics source does not read device communication.");

        public Task<IReadOnlyList<LivestockOperationRecord>> GetLivestockOperationsAsync(Guid cattleId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The analytics source reads ranch operations.");

        public Task<IReadOnlyList<LivestockOperationRecord>> GetRanchLivestockOperationsAsync(Guid ranchId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<LivestockOperationRecord>>(Operations);

        public Task<OpenAlertCounts> GetOpenAlertCountsAsync(Guid ranchId, CancellationToken cancellationToken) => Task.FromResult(Counts);

        public Task<ClinicalInterventionRecord?> FindClinicalInterventionAsync(Guid operationId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The analytics source does not resolve clinical interventions.");
    }
}
