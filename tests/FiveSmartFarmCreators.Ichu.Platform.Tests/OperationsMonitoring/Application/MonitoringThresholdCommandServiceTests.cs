using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.OperationsMonitoring.Application;

public class MonitoringThresholdCommandServiceTests
{
    private readonly FakeThresholds _thresholds = new();
    private readonly FakeRevisions _revisions = new();
    private readonly MonitoringThresholdCommandService _service;

    public MonitoringThresholdCommandServiceTests()
    {
        _service = new MonitoringThresholdCommandService(new FakeAccess(), _thresholds, _revisions, new FakeUnitOfWork());
    }

    [Fact]
    public async Task WarningAndCriticalLevelsOfTheSameMetricAreStoredSeparately()
    {
        await Configure(48, 84, AlertSeverity.Warning);

        await Configure(40, 90, AlertSeverity.Critical);

        Assert.Equal(2, _thresholds.Stored.Count);
        Assert.Equal(2, _revisions.Advances);
    }

    [Fact]
    public async Task ReconfiguringOneLevelReplacesOnlyThatSeverity()
    {
        await Configure(48, 84, AlertSeverity.Warning);
        await Configure(40, 90, AlertSeverity.Critical);

        await Configure(50, 80, AlertSeverity.Warning);

        Assert.Equal(2, _thresholds.Stored.Count);
        var warning = Assert.Single(_thresholds.Stored, threshold => threshold.Severity == AlertSeverity.Warning);
        Assert.Equal(50, warning.Range.MinValue);
        Assert.Equal(80, warning.Range.MaxValue);
    }

    [Fact]
    public async Task CriticalRangeThatDoesNotContainTheWarningRangeIsRejectedWithoutAdvancingTheRevision()
    {
        await Configure(48, 84, AlertSeverity.Warning);

        var result = await Configure(50, 90, AlertSeverity.Critical);

        Assert.Equal(OperationsMonitoringErrors.ThresholdLevelsInconsistent.Code, result.DomainError.Code);
        Assert.Single(_thresholds.Stored);
        Assert.Equal(1, _revisions.Advances);
    }

    [Fact]
    public async Task WarningRangeOutsideTheCriticalRangeIsRejected()
    {
        await Configure(40, 90, AlertSeverity.Critical);

        var result = await Configure(35, 84, AlertSeverity.Warning);

        Assert.Equal(OperationsMonitoringErrors.ThresholdLevelsInconsistent.Code, result.DomainError.Code);
    }

    private Task<Result<MonitoringThreshold>> Configure(double minValue, double maxValue, AlertSeverity severity) =>
        _service.Handle(new ConfigureThresholdCommand(TelemetryFixtures.RanchId, "MilkingCow", MonitoredMetric.HeartRate,
            ThresholdRange.Create(minValue, maxValue).Value!, severity), CancellationToken.None);

    private sealed class FakeAccess : IMonitoringAccessControl
    {
        public Task<Guid?> GetCurrentAccountIdAsync(CancellationToken cancellationToken) => Task.FromResult<Guid?>(Guid.NewGuid());

        public Task<bool> CanAccessRanchAsync(Guid accountId, Guid ranchId, MonitoringPermission permission, CancellationToken cancellationToken) =>
            Task.FromResult(true);
    }

    private sealed class FakeThresholds : IMonitoringThresholdRepository
    {
        public List<MonitoringThreshold> Stored { get; } = [];

        public Task AddAsync(MonitoringThreshold threshold, CancellationToken cancellationToken = default)
        {
            Stored.Add(threshold);
            return Task.CompletedTask;
        }

        public Task<MonitoringThreshold?> FindByRanchLifeStageAndMetricAndSeverityAsync(Guid ranchId, string lifeStage, MonitoredMetric metric, AlertSeverity severity, CancellationToken cancellationToken = default) =>
            Task.FromResult(Stored.SingleOrDefault(t => t.RanchId == ranchId && t.LifeStage == lifeStage && t.Metric == metric && t.Severity == severity));

        public Task<IReadOnlyList<MonitoringThreshold>> FindByRanchLifeStageAndMetricAsync(Guid ranchId, string lifeStage, MonitoredMetric metric, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<MonitoringThreshold>>(Stored.Where(t => t.RanchId == ranchId && t.LifeStage == lifeStage && t.Metric == metric).ToList());

        public Task<IReadOnlyList<MonitoringThreshold>> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The configuration does not list thresholds.");
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
