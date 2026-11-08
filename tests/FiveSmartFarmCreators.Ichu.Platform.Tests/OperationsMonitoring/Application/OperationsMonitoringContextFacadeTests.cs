using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Acl;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

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

    // The clinical lookup reads only the operations repository, so the other two are not needed here.
    private static OperationsMonitoringContextFacade CreateFacade(ILivestockOperationRepository operations) =>
        new(null!, null!, operations);

    private static LivestockOperation Operation(Guid operationId, OperationType type) =>
        LivestockOperation.Create(operationId, RanchId, CattleId, type, OccurredAt, AuthorId, null, null, null, OccurredAt).Value!;

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
            throw new NotSupportedException("The clinical lookup does not list operations by animal.");
    }
}
