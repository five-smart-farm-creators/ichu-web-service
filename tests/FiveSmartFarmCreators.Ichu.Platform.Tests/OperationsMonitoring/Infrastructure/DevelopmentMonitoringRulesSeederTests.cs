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

    [Fact]
    public async Task AnAlreadySeededRanchReceivesOnlyTheMissingHeartRateThresholds()
    {
        var thresholds = new FakeThresholds(Configured(MonitoredMetric.BodyTemperature, MonitoredMetric.ActivityIndex));
        var revisions = new FakeRevisions();
        var seeder = new DevelopmentMonitoringRulesSeeder(thresholds, new FakeGeofences(), revisions, new FakeUnitOfWork());

        await seeder.SeedAsync(CancellationToken.None);

        Assert.Equal(LifeStages, thresholds.Added.Select(threshold => threshold.LifeStage).ToArray());
        Assert.All(thresholds.Added, threshold =>
        {
            Assert.Equal(MonitoredMetric.HeartRate, threshold.Metric);
            Assert.Equal(48, threshold.Range.MinValue);
            Assert.Equal(84, threshold.Range.MaxValue);
            Assert.Equal(AlertSeverity.Warning, threshold.Severity);
            Assert.Equal(1, threshold.Revision);
        });
        Assert.Equal(1, revisions.Advances);
    }

    [Fact]
    public async Task ARanchWithEveryDevelopmentRuleIsLeftUnchanged()
    {
        var thresholds = new FakeThresholds(Configured(MonitoredMetric.BodyTemperature, MonitoredMetric.ActivityIndex, MonitoredMetric.HeartRate));
        var revisions = new FakeRevisions();
        var seeder = new DevelopmentMonitoringRulesSeeder(thresholds, new FakeGeofences(), revisions, new FakeUnitOfWork());

        await seeder.SeedAsync(CancellationToken.None);

        Assert.Empty(thresholds.Added);
        Assert.Equal(0, revisions.Advances);
    }

    private static List<MonitoringThreshold> Configured(params MonitoredMetric[] metrics) =>
        [.. LifeStages.SelectMany(lifeStage => metrics.Select(metric => Threshold(lifeStage, metric)))];

    private static MonitoringThreshold Threshold(string lifeStage, MonitoredMetric metric) =>
        MonitoringThreshold.Create(DevelopmentSeed.RanchId, lifeStage, metric, ThresholdRange.Create(1, 2).Value!, AlertSeverity.Warning, 1).Value!;

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

        public Task<MonitoringThreshold?> FindByRanchLifeStageAndMetricAsync(Guid ranchId, string lifeStage, MonitoredMetric metric, CancellationToken cancellationToken = default) =>
            Task.FromResult(configured.Concat(Added).SingleOrDefault(t => t.RanchId == ranchId && t.LifeStage == lifeStage && t.Metric == metric));

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
