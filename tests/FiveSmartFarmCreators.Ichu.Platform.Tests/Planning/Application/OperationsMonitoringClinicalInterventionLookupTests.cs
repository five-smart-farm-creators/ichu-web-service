using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Planning.Application;

public class OperationsMonitoringClinicalInterventionLookupTests
{
    private static readonly Guid InterventionId = Guid.Parse("66666666-6666-4666-8666-666666666666");
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid CattleId = Guid.Parse("33333333-3333-4333-8333-333333333333");

    [Fact]
    public async Task FindAsync_MapsTheClinicalInterventionToItsSnapshot()
    {
        var facade = new FakeOperationsMonitoringFacade
        {
            Intervention = new ClinicalInterventionRecord(InterventionId, RanchId, CattleId, "Treatment", DateTimeOffset.UtcNow)
        };

        var snapshot = await new OperationsMonitoringClinicalInterventionLookup(facade).FindAsync(InterventionId, CancellationToken.None);

        Assert.Equal(new ClinicalInterventionSnapshot(InterventionId, RanchId, CattleId), snapshot);
    }

    [Fact]
    public async Task FindAsync_ReturnsNullWhenOperationsMonitoringHasNoClinicalIntervention()
    {
        var snapshot = await new OperationsMonitoringClinicalInterventionLookup(new FakeOperationsMonitoringFacade())
            .FindAsync(InterventionId, CancellationToken.None);

        Assert.Null(snapshot);
    }

    private sealed class FakeOperationsMonitoringFacade : IOperationsMonitoringContextFacade
    {
        public ClinicalInterventionRecord? Intervention { get; init; }

        public Task<ClinicalInterventionRecord?> FindClinicalInterventionAsync(Guid operationId, CancellationToken cancellationToken) =>
            Task.FromResult(Intervention);

        public Task<IReadOnlyList<MonitoringReadingRecord>> GetReadingsAsync(Guid ranchId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken) =>
            throw new NotSupportedException("Planning only resolves clinical interventions.");

        public Task<IReadOnlyList<MonitoringAlertRecord>> GetAlertsAsync(Guid ranchId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken) =>
            throw new NotSupportedException("Planning only resolves clinical interventions.");

        public Task<DeviceCommunicationRecord?> GetLastDeviceCommunicationAsync(Guid deviceId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("Planning only resolves clinical interventions.");

        public Task<IReadOnlyList<LivestockOperationRecord>> GetLivestockOperationsAsync(Guid cattleId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken) =>
            throw new NotSupportedException("Planning only resolves clinical interventions.");

        public Task<IReadOnlyList<LivestockOperationRecord>> GetRanchLivestockOperationsAsync(Guid ranchId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken) =>
            throw new NotSupportedException("Planning only resolves clinical interventions.");

        public Task<OpenAlertCounts> GetOpenAlertCountsAsync(Guid ranchId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("Planning only resolves clinical interventions.");
    }
}
