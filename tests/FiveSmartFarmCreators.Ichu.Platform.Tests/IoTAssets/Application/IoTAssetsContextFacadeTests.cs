using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Acl;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using Microsoft.Extensions.Time.Testing;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.IoTAssets.Application;

public class IoTAssetsContextFacadeTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid CattleId = Guid.Parse("33333333-3333-4333-8333-333333333333");
    private static readonly Guid WaterTroughId = Guid.Parse("44444444-4444-4444-8444-444444444444");
    private static readonly DateTimeOffset At = new(2026, 11, 7, 10, 0, 0, TimeSpan.Zero);

    private readonly FakeIoTDeviceRepository _devices = new();
    private readonly FakeEdgeGatewayRepository _gateways = new();
    private readonly FakeTimeProvider _clock = new(At);

    // The tested members never reach the device command and query services, so they are not supplied.
    private IoTAssetsContextFacade CreateFacade() =>
        new(null!, null!, null!, _devices, _gateways, new AllowAllAccess(), _clock);

    [Fact]
    public async Task AuthenticateEdgeGatewayAsync_AcceptsTheKeyIssuedAtRegistration()
    {
        var registration = await RegisterGatewayAsync();

        var snapshot = await CreateFacade().AuthenticateEdgeGatewayAsync(registration.Key, CancellationToken.None);

        Assert.Equal(new EdgeGatewaySnapshot(registration.Gateway.Id.Value, RanchId), snapshot);
    }

    [Theory]
    [InlineData("")]
    [InlineData("no-separator")]
    [InlineData(".leading-separator")]
    [InlineData("not-a-guid.some-secret")]
    public async Task AuthenticateEdgeGatewayAsync_RejectsMalformedKeys(string presentedKey)
    {
        var snapshot = await CreateFacade().AuthenticateEdgeGatewayAsync(presentedKey, CancellationToken.None);

        Assert.Null(snapshot);
    }

    [Fact]
    public async Task AuthenticateEdgeGatewayAsync_RejectsUnknownIdentifiersAndWrongSecrets()
    {
        var registration = await RegisterGatewayAsync();
        var secret = registration.Key[(registration.Key.IndexOf('.') + 1)..];
        var facade = CreateFacade();

        Assert.Null(await facade.AuthenticateEdgeGatewayAsync($"{Guid.NewGuid():D}.{secret}", CancellationToken.None));
        Assert.Null(await facade.AuthenticateEdgeGatewayAsync($"{registration.Gateway.Id.Value:D}.wrong-secret", CancellationToken.None));
    }

    [Fact]
    public async Task AuthenticateEdgeGatewayAsync_RejectsARevokedGateway()
    {
        var registration = await RegisterGatewayAsync();
        await CreateCommandService().HandleAsync(new RevokeEdgeGatewayCommand(RanchId, registration.Gateway.Id), CancellationToken.None);

        var snapshot = await CreateFacade().AuthenticateEdgeGatewayAsync(registration.Key, CancellationToken.None);

        Assert.Null(snapshot);
    }

    [Fact]
    public async Task FindDeviceAttributionAsync_ReturnsTheCattleCoveringTheInstant()
    {
        var collar = AddDevice(DeviceType.SmartCollar, CattleId, assignedAt: At.AddDays(-1));

        var attribution = await CreateFacade().FindDeviceAttributionAsync(collar.Id.Value, At, CancellationToken.None);

        Assert.Equal(new DeviceAttributionSnapshot(collar.Id.Value, RanchId, CattleId), attribution);
    }

    [Fact]
    public async Task FindDeviceAttributionAsync_OmitsTheCattleBeforeTheAssignmentStarts()
    {
        var collar = AddDevice(DeviceType.SmartCollar, CattleId, assignedAt: At.AddDays(-1));

        var attribution = await CreateFacade().FindDeviceAttributionAsync(collar.Id.Value, At.AddDays(-2), CancellationToken.None);

        Assert.Equal(new DeviceAttributionSnapshot(collar.Id.Value, RanchId, null), attribution);
    }

    [Fact]
    public async Task FindDeviceAttributionAsync_OmitsTheCattleAfterTheAssignmentIsReleased()
    {
        var collar = AddDevice(DeviceType.SmartCollar, CattleId, assignedAt: At.AddDays(-1));
        collar.Release("Sold", At.AddHours(-1));

        var attribution = await CreateFacade().FindDeviceAttributionAsync(collar.Id.Value, At, CancellationToken.None);

        Assert.Equal(new DeviceAttributionSnapshot(collar.Id.Value, RanchId, null), attribution);
    }

    [Fact]
    public async Task FindDeviceAttributionAsync_OmitsTheCattleForAWaterTroughTarget()
    {
        var controller = AddDevice(DeviceType.WaterController, WaterTroughId, assignedAt: At.AddDays(-1));

        var attribution = await CreateFacade().FindDeviceAttributionAsync(controller.Id.Value, At, CancellationToken.None);

        Assert.Equal(new DeviceAttributionSnapshot(controller.Id.Value, RanchId, null), attribution);
    }

    [Fact]
    public async Task FindDeviceAttributionAsync_ReturnsNullForAnUnknownDevice()
    {
        var attribution = await CreateFacade().FindDeviceAttributionAsync(Guid.NewGuid(), At, CancellationToken.None);

        Assert.Null(attribution);
    }

    private EdgeGatewayCommandService CreateCommandService() =>
        new(_gateways, new AllowAllAccess(), new DirectTransaction(), _clock);

    private async Task<EdgeGatewayRegistration> RegisterGatewayAsync()
    {
        var result = await CreateCommandService().HandleAsync(new RegisterEdgeGatewayCommand(RanchId, "Barn gateway"), CancellationToken.None);
        return result.Value!;
    }

    private IoTDevice AddDevice(DeviceType type, Guid targetId, DateTimeOffset assignedAt)
    {
        var device = IoTDevice.Register(
            new RegisterDeviceCommand(RanchId, $"DEV-{Guid.NewGuid():N}"[..16], type, null), assignedAt.AddDays(-1)).Value!;
        device.AssignTo(targetId, assignedAt);
        _devices.Items.Add(device);
        return device;
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
}
