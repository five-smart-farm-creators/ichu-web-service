using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using Microsoft.Extensions.Time.Testing;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.IoTAssets.Application;

public class DeviceCommandServiceTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid FirstCattleId = Guid.Parse("33333333-3333-4333-8333-333333333333");
    private static readonly Guid SecondCattleId = Guid.Parse("55555555-5555-4555-8555-555555555555");
    private static readonly DateTimeOffset At = new(2026, 11, 7, 10, 0, 0, TimeSpan.Zero);

    private readonly FakeDeviceRepository _devices = new();
    private readonly FakeCattleLookup _cattle = new();
    private readonly FakeEntitlementPort _entitlement = new();

    private DeviceCommandService CreateService() =>
        new(_devices, new FakeWaterTroughRepository(), new AllowAllAccess(), _cattle, _entitlement,
            new DirectTransaction(), new DeviceAssignmentDomainService(), new FakeTimeProvider(At));

    [Fact]
    public async Task AssignCollar_WithinThePlanLimitAssignsTheDevice()
    {
        var collar = AddCollar("COL-002");
        _cattle.Target = new CattleAssignmentTarget(SecondCattleId, RanchId, IsActive: true);
        _entitlement.Entitlement = new CollarEntitlement(true, MaximumCollars: 2);
        AddCollarAssignedTo(FirstCattleId, "COL-001");

        var result = await CreateService().HandleAsync(AssignCommand(collar, SecondCattleId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(DeviceStatus.InService, result.Value!.Status);
    }

    [Fact]
    public async Task AssignCollar_WhenThePlanLimitIsReachedDoesNotAssignTheDevice()
    {
        var collar = AddCollar("COL-002");
        _cattle.Target = new CattleAssignmentTarget(SecondCattleId, RanchId, IsActive: true);
        _entitlement.Entitlement = new CollarEntitlement(true, MaximumCollars: 1);
        AddCollarAssignedTo(FirstCattleId, "COL-001");

        var result = await CreateService().HandleAsync(AssignCommand(collar, SecondCattleId), CancellationToken.None);

        Assert.Equal(IoTAssetsErrors.DeviceLimitReached.Code, result.DomainError.Code);
        Assert.DoesNotContain(collar.Assignments, assignment => assignment.IsOpen());
    }

    [Fact]
    public async Task AssignCollar_ToAnInactiveAnimalIsRefused()
    {
        var collar = AddCollar("COL-002");
        _cattle.Target = new CattleAssignmentTarget(SecondCattleId, RanchId, IsActive: false);
        _entitlement.Entitlement = new CollarEntitlement(true, MaximumCollars: 10);

        var result = await CreateService().HandleAsync(AssignCommand(collar, SecondCattleId), CancellationToken.None);

        Assert.Equal(IoTAssetsErrors.InvalidState.Code, result.DomainError.Code);
    }

    [Fact]
    public async Task AssignCollar_WithoutAnActiveSubscriptionIsRefused()
    {
        var collar = AddCollar("COL-002");
        _cattle.Target = new CattleAssignmentTarget(SecondCattleId, RanchId, IsActive: true);
        _entitlement.Entitlement = new CollarEntitlement(false, MaximumCollars: 0);

        var result = await CreateService().HandleAsync(AssignCommand(collar, SecondCattleId), CancellationToken.None);

        Assert.Equal(IoTAssetsErrors.SubscriptionRequired.Code, result.DomainError.Code);
    }

    private static AssignDeviceCommand AssignCommand(IoTDevice collar, Guid targetId) =>
        new(RanchId, collar.Id, targetId, collar.Version);

    private IoTDevice AddCollar(string serial)
    {
        var collar = IoTDevice.Register(new RegisterDeviceCommand(RanchId, serial, DeviceType.SmartCollar, null), At).Value!;
        _devices.Items.Add(collar);
        return collar;
    }

    private void AddCollarAssignedTo(Guid cattleId, string serial)
    {
        var collar = AddCollar(serial);
        collar.AssignTo(cattleId, At.AddDays(-1));
    }

    private sealed class AllowAllAccess : IIoTAssetsAccessService
    {
        public Task<Result> EnsureReadAccessAsync(Guid ranchId, CancellationToken cancellationToken) => Task.FromResult(Result.Success());

        public Task<Result> EnsureManageAccessAsync(Guid ranchId, CancellationToken cancellationToken) => Task.FromResult(Result.Success());

        public Task<Result> EnsureCommunicationAccessAsync(Guid ranchId, Guid deviceId, CancellationToken cancellationToken) =>
            Task.FromResult(Result.Success());
    }

    private sealed class DirectTransaction : IIoTAssetsTransaction
    {
        public Task<Result<T>> ExecuteAsync<T>(Func<CancellationToken, Task<Result<T>>> operation, CancellationToken cancellationToken) =>
            operation(cancellationToken);
    }

    private sealed class FakeCattleLookup : ICattleLookupPort
    {
        public CattleAssignmentTarget? Target { get; set; }

        public Task<Result<CattleAssignmentTarget>> GetCattleAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken) =>
            Task.FromResult(Target is null
                ? Result<CattleAssignmentTarget>.Failure(IoTAssetsErrors.TargetNotFound)
                : Result<CattleAssignmentTarget>.Success(Target));

        public Task<Result<bool>> LotExistsInRanchAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken) =>
            Task.FromResult(Result<bool>.Success(true));
    }

    private sealed class FakeEntitlementPort : ISubscriptionEntitlementPort
    {
        public CollarEntitlement Entitlement { get; set; } = new(false, 0);

        public Task<Result<CollarEntitlement>> GetAsync(Guid ranchId, CancellationToken cancellationToken) =>
            Task.FromResult(Result<CollarEntitlement>.Success(Entitlement));
    }

    private sealed class FakeWaterTroughRepository : IWaterTroughRepository
    {
        public Task AddAsync(WaterTrough waterTrough, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<WaterTrough?> FindByIdAsync(Guid ranchId, WaterTroughId waterTroughId, CancellationToken cancellationToken) =>
            Task.FromResult<WaterTrough?>(null);

        public Task<WaterTrough?> FindByIdAsNoTrackingAsync(Guid ranchId, WaterTroughId waterTroughId, CancellationToken cancellationToken) =>
            Task.FromResult<WaterTrough?>(null);

        public Task<IReadOnlyList<WaterTrough>> FindByRanchAsNoTrackingAsync(Guid ranchId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<WaterTrough>>([]);
    }

    private sealed class FakeDeviceRepository : IIoTDeviceRepository
    {
        public List<IoTDevice> Items { get; } = [];

        public Task AddAsync(IoTDevice device, CancellationToken cancellationToken)
        {
            Items.Add(device);
            return Task.CompletedTask;
        }

        public Task<IoTDevice?> FindByIdAsync(Guid ranchId, DeviceId deviceId, CancellationToken cancellationToken) =>
            Task.FromResult(Items.SingleOrDefault(device => device.RanchId == ranchId && device.Id == deviceId));

        public Task<IoTDevice?> FindByIdAsNoTrackingAsync(Guid ranchId, DeviceId deviceId, CancellationToken cancellationToken) =>
            FindByIdAsync(ranchId, deviceId, cancellationToken);

        public Task<IReadOnlyList<IoTDevice>> FindByRanchAsNoTrackingAsync(Guid ranchId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<IoTDevice>>(Items.Where(device => device.RanchId == ranchId).ToList());

        public Task<bool> ExistsBySerialAsync(DeviceSerial serial, CancellationToken cancellationToken) =>
            Task.FromResult(Items.Any(device => device.Serial.Value == serial.Value));

        public Task<bool> HasOpenAssignmentAsync(Guid ranchId, Guid targetId, AssignmentTargetType targetType, CancellationToken cancellationToken) =>
            Task.FromResult(Items.Any(device => device.RanchId == ranchId
                && device.Assignments.Any(assignment => assignment.IsOpen() && assignment.TargetId == targetId && assignment.TargetType == targetType)));

        public Task<int> CountAssignedCollarsAsync(Guid ranchId, CancellationToken cancellationToken) =>
            Task.FromResult(Items.Count(device => device.RanchId == ranchId && device.Type == DeviceType.SmartCollar
                && device.Assignments.Any(assignment => assignment.IsOpen())));

        public Task<IoTDevice?> FindByAssignedCattleAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken) =>
            Task.FromResult(Items.SingleOrDefault(device => device.RanchId == ranchId
                && device.Assignments.Any(assignment => assignment.IsOpen() && assignment.TargetId == cattleId)));

        public Task<IoTDevice?> FindByAssignedCattleAsNoTrackingAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken) =>
            FindByAssignedCattleAsync(ranchId, cattleId, cancellationToken);
    }
}
