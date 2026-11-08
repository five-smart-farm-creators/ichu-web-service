using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Acl;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.OperationsMonitoring.Application;

public class OperationsMonitoringContextFacadeTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid CattleId = Guid.Parse("33333333-3333-4333-8333-333333333333");
    private static readonly Guid AuthorId = Guid.Parse("44444444-4444-4444-8444-444444444444");
    private static readonly DateTimeOffset OccurredAt = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task FindClinicalIntervention_ReturnsAClinicalOperation()
    {
        var operations = new FakeLivestockOperations();
        var operationId = Guid.NewGuid();
        operations.Stored.Add(Operation(operationId, OperationType.Vaccination));

        var record = await CreateFacade(operations).FindClinicalInterventionAsync(operationId, CancellationToken.None);

        Assert.NotNull(record);
        Assert.Equal(operationId, record.OperationId);
        Assert.Equal(RanchId, record.RanchId);
        Assert.Equal(CattleId, record.CattleId);
        Assert.Equal("Vaccination", record.Type);
    }

    [Fact]
    public async Task FindClinicalIntervention_ReturnsNullForAFieldEvent()
    {
        var operations = new FakeLivestockOperations();
        var operationId = Guid.NewGuid();
        operations.Stored.Add(Operation(operationId, OperationType.HealthEvent));

        var record = await CreateFacade(operations).FindClinicalInterventionAsync(operationId, CancellationToken.None);

        Assert.Null(record);
    }

    [Fact]
    public async Task FindClinicalIntervention_ReturnsNullForAnUnknownOperation()
    {
        var record = await CreateFacade(new FakeLivestockOperations())
            .FindClinicalInterventionAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(record);
    }

    [Fact]
    public async Task GetRanchLivestockOperations_ReturnsTheOperationsOfTheRanchInsideThePeriod()
    {
        var operations = new FakeLivestockOperations();
        operations.Stored.Add(Operation(Guid.NewGuid(), OperationType.Birth));
        operations.Stored.Add(Operation(Guid.NewGuid(), OperationType.Vaccination));
        operations.Stored.Add(Operation(Guid.NewGuid(), OperationType.Birth, OccurredAt.AddDays(30)));

        var records = await CreateFacade(operations).GetRanchLivestockOperationsAsync(
            RanchId, OccurredAt.AddHours(-1), OccurredAt.AddDays(1), CancellationToken.None);

        Assert.Equal(2, records.Count);
        Assert.Contains(records, record => record.Type == OperationType.Birth && !record.IsClinical);
        Assert.Contains(records, record => record.Type == OperationType.Vaccination && record.IsClinical);
    }

    [Fact]
    public async Task GetOpenAlertCounts_MapsEachSeverityToItsNumberOfOpenAlerts()
    {
        var alerts = new FakeMonitoringAlerts
        {
            OpenBySeverity = new Dictionary<AlertSeverity, int> { [AlertSeverity.Critical] = 2, [AlertSeverity.Warning] = 1 }
        };

        var counts = await CreateFacade(alerts).GetOpenAlertCountsAsync(RanchId, CancellationToken.None);

        Assert.Equal(new OpenAlertCounts(2, 1, 0), counts);
    }

    // Each read is tested with only the repository it uses, so the other repositories are not supplied.
    private static OperationsMonitoringContextFacade CreateFacade(ILivestockOperationRepository operations) =>
        new(null!, null!, operations);

    private static OperationsMonitoringContextFacade CreateFacade(IMonitoringAlertRepository alerts) =>
        new(null!, alerts, null!);

    private static LivestockOperation Operation(Guid operationId, OperationType type, DateTimeOffset? occurredAt = null)
    {
        var at = occurredAt ?? OccurredAt;
        return LivestockOperation.Create(operationId, RanchId, CattleId, type, at, AuthorId, null, null, null, at).Value!;
    }

    private sealed class FakeLivestockOperations : ILivestockOperationRepository
    {
        public List<LivestockOperation> Stored { get; } = [];

        public Task AddAsync(LivestockOperation operation, CancellationToken cancellationToken = default)
        {
            Stored.Add(operation);
            return Task.CompletedTask;
        }

        public Task<LivestockOperation?> FindByIdAsync(LivestockOperationId operationId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Stored.SingleOrDefault(operation => operation.Id == operationId));

        public Task<IReadOnlyList<LivestockOperation>> FindByCattleAsync(Guid cattleId, TimePeriod period, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The ranch reads do not list operations by animal.");

        public Task<IReadOnlyList<LivestockOperation>> FindByRanchAndPeriodAsNoTrackingAsync(Guid ranchId, TimePeriod period, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<LivestockOperation>>(Stored
                .Where(operation => operation.RanchId == ranchId && operation.OccurredAt >= period.From && operation.OccurredAt < period.To)
                .ToList());
    }

    private sealed class FakeMonitoringAlerts : IMonitoringAlertRepository
    {
        public IReadOnlyDictionary<AlertSeverity, int> OpenBySeverity { get; init; } = new Dictionary<AlertSeverity, int>();

        public Task<IReadOnlyDictionary<AlertSeverity, int>> CountOpenByRanchAndSeverityAsync(Guid ranchId, CancellationToken cancellationToken = default) =>
            Task.FromResult(OpenBySeverity);

        public Task AddAsync(MonitoringAlert alert, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The open counts only read alerts.");

        public Task<MonitoringAlert?> FindByIdAsync(MonitoringAlertId alertId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The open counts only read alerts.");

        public Task<MonitoringAlert?> FindByIdAsNoTrackingAsync(MonitoringAlertId alertId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The open counts only read alerts.");

        public Task<MonitoringAlert?> FindByOperationIdAsync(Guid operationId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The open counts only read alerts.");

        public Task<IReadOnlyList<MonitoringAlert>> FindPendingByCattleAndTypeAsync(Guid cattleId, AlertType type, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The open counts only read alerts.");

        public Task<IReadOnlyList<MonitoringAlert>> FindByRanchAsync(Guid ranchId, AlertStatus? status, Guid? cattleId, int skip, int take, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The open counts only read alerts.");

        public Task<int> CountByRanchAsync(Guid ranchId, AlertStatus? status, Guid? cattleId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The open counts only read alerts.");

        public Task<IReadOnlyList<MonitoringAlert>> FindActiveDuringAsync(Guid ranchId, TimePeriod period, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The open counts only read alerts.");
    }
}
