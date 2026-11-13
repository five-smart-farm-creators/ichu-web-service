using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using CattleInformationSnapshot = FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl.CattleSnapshot;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.OperationsMonitoring.Application;

public class IoTAssetsEdgeDeviceAssignmentLookupTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid DeviceId = Guid.Parse("10000000-0000-4000-8000-000000000001");
    private static readonly Guid OtherDeviceId = Guid.Parse("20000000-0000-4000-8000-000000000002");
    private static readonly Guid CattleId = Guid.Parse("33333333-3333-4333-8333-333333333333");
    private static readonly Guid ExitedCattleId = Guid.Parse("55555555-5555-4555-8555-555555555555");
    private static readonly Guid LotId = Guid.Parse("6a1f2c0e-0d3b-4c51-9a77-1b2c3d4e5f60");

    [Fact]
    public async Task FindAsync_PairsEachCollarWithTheLifeStageAndLotOfItsAnimal()
    {
        var iotAssets = new FakeIoTAssetsFacade(Assignment(DeviceId, CattleId));
        var cattle = new FakeCattleInformationFacade(new CattlePlacementSnapshot(CattleId, LotId, "MilkingCow"));

        var assignments = await new IoTAssetsEdgeDeviceAssignmentLookup(iotAssets, cattle).FindAsync(RanchId, CancellationToken.None);

        Assert.Equal(new EdgeDeviceAssignment(DeviceId, CattleId, LotId, "MilkingCow"), Assert.Single(assignments));
    }

    [Fact]
    public async Task FindAsync_LeavesTheLotEmptyForAnAnimalWithoutLot()
    {
        var iotAssets = new FakeIoTAssetsFacade(Assignment(DeviceId, CattleId));
        var cattle = new FakeCattleInformationFacade(new CattlePlacementSnapshot(CattleId, null, "Heifer"));

        var assignment = Assert.Single(await new IoTAssetsEdgeDeviceAssignmentLookup(iotAssets, cattle).FindAsync(RanchId, CancellationToken.None));

        Assert.Null(assignment.LotId);
        Assert.Equal("Heifer", assignment.LifeStage);
    }

    [Fact]
    public async Task FindAsync_SkipsCollarsWhoseAnimalIsNotActive()
    {
        var iotAssets = new FakeIoTAssetsFacade(Assignment(DeviceId, CattleId), Assignment(OtherDeviceId, ExitedCattleId));
        var cattle = new FakeCattleInformationFacade(new CattlePlacementSnapshot(CattleId, LotId, "MilkingCow"));

        var assignment = Assert.Single(await new IoTAssetsEdgeDeviceAssignmentLookup(iotAssets, cattle).FindAsync(RanchId, CancellationToken.None));

        Assert.Equal(DeviceId, assignment.DeviceId);
    }

    private static DeviceAssignmentSnapshot Assignment(Guid deviceId, Guid cattleId) =>
        new(Guid.NewGuid(), deviceId, RanchId, cattleId, "Cattle", new DateTimeOffset(2026, 11, 1, 8, 0, 0, TimeSpan.Zero), null);

    private sealed class FakeIoTAssetsFacade(params DeviceAssignmentSnapshot[] assignments) : IIoTAssetsContextFacade
    {
        public Task<IReadOnlyList<DeviceAssignmentSnapshot>> FindOpenCollarAssignmentsAsync(Guid ranchId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<DeviceAssignmentSnapshot>>(assignments);

        public Task<Result<DeviceAssignmentSnapshot?>> GetCurrentCollarAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The lookup reads open collar assignments only.");

        public Task<Result<DeviceAssignmentSnapshot?>> ResolveAssignmentAtAsync(Guid ranchId, Guid deviceId, DateTimeOffset instant,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException("The lookup reads open collar assignments only.");

        public Task<Result<IReadOnlyList<DeviceAssignmentSnapshot>>> GetActiveAssignmentsAsync(Guid ranchId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The lookup reads open collar assignments only.");

        public Task<Result<WaterTroughConfiguration>> GetWaterTroughConfigurationAsync(Guid ranchId, Guid waterTroughId,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException("The lookup reads open collar assignments only.");

        public Task<Result> RegisterCommunicationAsync(Guid ranchId, Guid deviceId, DateTimeOffset communicatedAt, DateTimeOffset receivedAt,
            int? batteryPercentage, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The lookup reads open collar assignments only.");

        public Task<Result<int>> ReleaseCollarForInactiveCattleAsync(Guid ranchId, Guid cattleId, DateTimeOffset deactivatedAt,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException("The lookup reads open collar assignments only.");

        public Task<EdgeGatewaySnapshot?> AuthenticateEdgeGatewayAsync(string presentedKey, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The lookup reads open collar assignments only.");

        public Task<DeviceAttributionSnapshot?> FindDeviceAttributionAsync(Guid deviceId, DateTimeOffset instant, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The lookup reads open collar assignments only.");
    }

    private sealed class FakeCattleInformationFacade(params CattlePlacementSnapshot[] placements) : ICattleInformationContextFacade
    {
        public Task<IReadOnlyList<CattlePlacementSnapshot>> FindActivePlacementsAsync(Guid ranchId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<CattlePlacementSnapshot>>(placements);

        public Task<CattleInformationSnapshot?> FindByIdAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The lookup reads active placements only.");

        public Task<Guid?> FindRanchIdForLotAsync(Guid lotId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The lookup reads active placements only.");

        public Task<bool> LotExistsAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The lookup reads active placements only.");

        public Task<IReadOnlyList<CattleInformationSnapshot>> FindActiveByLotAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The lookup reads active placements only.");

        public Task<CattleLifecycleSnapshot?> FindLifecycleAsync(Guid cattleId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The lookup reads active placements only.");

        public Task<IReadOnlyList<HerdMemberSnapshot>> GetHerdAsync(Guid ranchId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The lookup reads active placements only.");
    }
}
