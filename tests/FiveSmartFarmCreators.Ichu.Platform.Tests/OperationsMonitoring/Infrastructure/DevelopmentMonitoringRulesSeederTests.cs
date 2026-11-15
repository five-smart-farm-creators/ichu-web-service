using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Seeding;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Seeding;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.OperationsMonitoring.Infrastructure;

public class DevelopmentMonitoringRulesSeederTests
{
    private static readonly string[] LifeStages = ["MilkingCow", "Heifer"];

    // The levels that a database seeded before the heart rate Critical and methane levels existed already holds.
    private static readonly (MonitoredMetric Metric, AlertSeverity Severity)[] PreviousLevels =
    [
        (MonitoredMetric.BodyTemperature, AlertSeverity.Critical),
        (MonitoredMetric.ActivityIndex, AlertSeverity.Warning),
        (MonitoredMetric.HeartRate, AlertSeverity.Warning)
    ];

    private static readonly (MonitoredMetric Metric, AlertSeverity Severity)[] DevelopmentLevels =
    [
        .. PreviousLevels,
        (MonitoredMetric.HeartRate, AlertSeverity.Critical),
        (MonitoredMetric.MethaneIndex, AlertSeverity.Warning),
        (MonitoredMetric.MethaneIndex, AlertSeverity.Critical)
    ];

    [Fact]
    public async Task AnAlreadySeededRanchReceivesOnlyTheMissingLevelsUnderOneRevision()
    {
        var thresholds = new FakeThresholds(Configured(PreviousLevels));
        var revisions = new FakeRevisions();
        var seeder = new DevelopmentMonitoringRulesSeeder(thresholds, new FakeGeofences(), revisions, new FakeUnitOfWork());

        await seeder.SeedAsync(CancellationToken.None);

        var expected = LifeStages.SelectMany(lifeStage => new[]
        {
            (lifeStage, MonitoredMetric.HeartRate, AlertSeverity.Critical, 40d, 90d),
            (lifeStage, MonitoredMetric.MethaneIndex, AlertSeverity.Warning, 0d, 60d),
            (lifeStage, MonitoredMetric.MethaneIndex, AlertSeverity.Critical, 0d, 80d)
        }).ToList();
        var actual = thresholds.Added
            .Select(threshold => (threshold.LifeStage, threshold.Metric, threshold.Severity, threshold.Range.MinValue, threshold.Range.MaxValue))
            .ToList();
        Assert.Equal(expected, actual);
        Assert.All(thresholds.Added, threshold => Assert.Equal(1, threshold.Revision));
        Assert.Equal(1, revisions.Advances);
    }

    [Fact]
    public async Task ARanchWithEveryDevelopmentRuleIsLeftUnchanged()
    {
        var thresholds = new FakeThresholds(Configured(DevelopmentLevels));
        var revisions = new FakeRevisions();
        var seeder = new DevelopmentMonitoringRulesSeeder(thresholds, new FakeGeofences(), revisions, new FakeUnitOfWork());

        await seeder.SeedAsync(CancellationToken.None);

        Assert.Empty(thresholds.Added);
        Assert.Equal(0, revisions.Advances);
    }

    private static List<MonitoringThreshold> Configured((MonitoredMetric Metric, AlertSeverity Severity)[] levels) =>
        [.. LifeStages.SelectMany(lifeStage => levels.Select(level => Threshold(lifeStage, level.Metric, level.Severity)))];

    private static MonitoringThreshold Threshold(string lifeStage, MonitoredMetric metric, AlertSeverity severity) =>
        MonitoringThreshold.Create(DevelopmentSeed.RanchId, lifeStage, metric, ThresholdRange.Create(1, 2).Value!, severity, 1).Value!;

    private static Geofence PastureGeofence() =>
        Geofence.Create(DevelopmentSeed.RanchId, DevelopmentSeed.LotId, "Development pasture", 25, AlertSeverity.Warning,
            [
                GeoCoordinate.Create(-16.505, -68.105).Value!,
                GeoCoordinate.Create(-16.505, -68.095).Value!,
                GeoCoordinate.Create(-16.495, -68.095).Value!,
                GeoCoordinate.Create(-16.495, -68.105).Value!
            ], 1).Value!;

    private sealed class FakeThresholds(IReadOnlyList<MonitoringThreshold> configured) : IMonitoringThresholdRepository
    {
        public List<MonitoringThreshold> Added { get; } = [];

        public Task AddAsync(MonitoringThreshold threshold, CancellationToken cancellationToken = default)
        {
            Added.Add(threshold);
            return Task.CompletedTask;
        }

        public Task<MonitoringThreshold?> FindByRanchLifeStageAndMetricAndSeverityAsync(Guid ranchId, string lifeStage, MonitoredMetric metric, AlertSeverity severity, CancellationToken cancellationToken = default) =>
            Task.FromResult(configured.Concat(Added).SingleOrDefault(t =>
                t.RanchId == ranchId && t.LifeStage == lifeStage && t.Metric == metric && t.Severity == severity));

        public Task<IReadOnlyList<MonitoringThreshold>> FindByRanchLifeStageAndMetricAsync(Guid ranchId, string lifeStage, MonitoredMetric metric, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The seeder looks up one threshold per severity.");

        public Task<IReadOnlyList<MonitoringThreshold>> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The seeder looks up one threshold at a time.");
    }

    private sealed class FakeGeofences : IGeofenceRepository
    {
        // The Development pasture is already seeded, so only the thresholds can be missing.
        private readonly Geofence _pasture = PastureGeofence();

        public Task<Geofence?> FindByLotAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken = default) =>
            Task.FromResult<Geofence?>(_pasture);

        public Task AddAsync(Geofence geofence, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The pasture geofence is already seeded.");

        public Task<Geofence?> FindByIdAsync(GeofenceId geofenceId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The seeder looks up the geofence by lot.");

        public Task<IReadOnlyList<Geofence>> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The seeder looks up the geofence by lot.");
    }

    private sealed class FakeRevisions : IMonitoringRuleRevisionService
    {
        public int Advances { get; private set; }

        public Task<long> AdvanceAsync(Guid ranchId, CancellationToken cancellationToken = default)
        {
            Advances++;
            return Task.FromResult<long>(Advances);
        }
    }

    private sealed class FakeUnitOfWork : IOperationsMonitoringUnitOfWork
    {
        public Task<bool> TryCompleteAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
    }
}
