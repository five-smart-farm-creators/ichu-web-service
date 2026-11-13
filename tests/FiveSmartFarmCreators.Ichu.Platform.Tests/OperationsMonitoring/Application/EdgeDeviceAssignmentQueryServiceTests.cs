using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.OperationsMonitoring.Application;

public class EdgeDeviceAssignmentQueryServiceTests
{
    private static readonly Guid GatewayId = Guid.Parse("c07d8c6e-6d9b-4cb7-9add-7b8c9daebfc6");
    private static readonly Guid OtherGatewayId = Guid.Parse("d18e9d7f-7e0c-4dc8-8bbe-8d9c0eb0c7d0");
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid CattleId = Guid.Parse("33333333-3333-4333-8333-333333333333");
    private static readonly Guid OtherCattleId = Guid.Parse("44444444-4444-4444-8444-444444444444");
    private static readonly Guid LotId = Guid.Parse("6a1f2c0e-0d3b-4c51-9a77-1b2c3d4e5f60");
    private static readonly Guid EarlierDeviceId = Guid.Parse("10000000-0000-4000-8000-000000000001");
    private static readonly Guid LaterDeviceId = Guid.Parse("20000000-0000-4000-8000-000000000002");

    [Fact]
    public async Task Handle_WithoutVerifiedGatewayReturnsUnauthenticated()
    {
        var lookup = new FakeAssignmentLookup();
        var service = new EdgeDeviceAssignmentQueryService(new FakeGatewayAuthenticator(null), lookup);

        var result = await service.Handle(new GetEdgeDeviceAssignmentsQuery(GatewayId), CancellationToken.None);

        Assert.Equal(OperationsMonitoringErrors.GatewayUnauthenticated.Code, result.DomainError.Code);
        Assert.Null(lookup.LastRanchId);
    }

    [Fact]
    public async Task Handle_ForAnotherGatewayReturnsMismatch()
    {
        var lookup = new FakeAssignmentLookup();
        var service = new EdgeDeviceAssignmentQueryService(new FakeGatewayAuthenticator(new EdgeGatewayPrincipal(OtherGatewayId, RanchId)), lookup);

        var result = await service.Handle(new GetEdgeDeviceAssignmentsQuery(GatewayId), CancellationToken.None);

        Assert.Equal(OperationsMonitoringErrors.GatewayMismatch.Code, result.DomainError.Code);
        Assert.Null(lookup.LastRanchId);
    }

    [Fact]
    public async Task Handle_ReturnsTheCollarsOfTheRanchOrderedByDevice()
    {
        var earlier = new EdgeDeviceAssignment(EarlierDeviceId, CattleId, null, "Heifer");
        var later = new EdgeDeviceAssignment(LaterDeviceId, OtherCattleId, LotId, "MilkingCow");
        var lookup = new FakeAssignmentLookup { Assignments = [later, earlier] };
        var service = new EdgeDeviceAssignmentQueryService(new FakeGatewayAuthenticator(new EdgeGatewayPrincipal(GatewayId, RanchId)), lookup);

        var result = await service.Handle(new GetEdgeDeviceAssignmentsQuery(GatewayId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(RanchId, lookup.LastRanchId);
        Assert.Equal(RanchId, result.Value!.RanchId);
        Assert.Equal(new[] { earlier, later }, result.Value!.Assignments);
    }

    private sealed class FakeGatewayAuthenticator(EdgeGatewayPrincipal? principal) : IEdgeGatewayAuthenticator
    {
        public Task<EdgeGatewayPrincipal?> AuthenticateAsync(CancellationToken cancellationToken) => Task.FromResult(principal);
    }

    private sealed class FakeAssignmentLookup : IEdgeDeviceAssignmentLookup
    {
        public List<EdgeDeviceAssignment> Assignments { get; init; } = [];

        public Guid? LastRanchId { get; private set; }

        public Task<IReadOnlyList<EdgeDeviceAssignment>> FindAsync(Guid ranchId, CancellationToken cancellationToken)
        {
            LastRanchId = ranchId;
            return Task.FromResult<IReadOnlyList<EdgeDeviceAssignment>>(Assignments);
        }
    }
}
