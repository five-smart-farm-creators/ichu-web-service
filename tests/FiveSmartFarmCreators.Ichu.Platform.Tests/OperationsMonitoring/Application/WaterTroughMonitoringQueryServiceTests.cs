using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.OperationsMonitoring.Application;

public sealed class WaterTroughMonitoringQueryServiceTests
{
    private static readonly Guid AccountId = Guid.NewGuid();
    private static readonly Guid RanchId = Guid.NewGuid();
    private static readonly Guid TroughId = Guid.NewGuid();
    private static readonly Guid ControllerId = Guid.NewGuid();
    private static readonly Guid GatewayId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 11, 14, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetStatus_DistinguishesNoControllerAndNoReadings()
    {
        var noController = await Service([Snapshot(hasController: false)], []).Handle(
            new GetWaterTroughStatusQuery(RanchId, TroughId), CancellationToken.None);
        var noReadings = await Service([Snapshot()], []).Handle(
            new GetWaterTroughStatusQuery(RanchId, TroughId), CancellationToken.None);

        Assert.Equal(WaterControllerStatus.NoController, noController.Value!.ControllerStatus);
        Assert.Equal(WaterMeasurementStatus.Unavailable, noController.Value.MeasurementStatus);
        Assert.Equal(WaterTemperatureRangeStatus.Unavailable, noController.Value.RangeStatus);
        Assert.Equal(WaterControllerStatus.NoReadings, noReadings.Value!.ControllerStatus);
        Assert.Null(noReadings.Value.LatestReading);
    }

    [Fact]
    public async Task GetStatus_RetainsInvalidReadingButDoesNotCompareItToRange()
    {
        var reading = Reading(3m, valid: false, capturedAt: Now.AddMinutes(-1), receivedAt: Now.AddMinutes(-1));

        var result = await Service([Snapshot()], [reading]).Handle(new GetWaterTroughStatusQuery(RanchId, TroughId),
            CancellationToken.None);

        Assert.Equal(WaterControllerStatus.Communicating, result.Value!.ControllerStatus);
        Assert.Equal(WaterMeasurementStatus.Invalid, result.Value.MeasurementStatus);
        Assert.Equal(WaterTemperatureRangeStatus.Unavailable, result.Value.RangeStatus);
        Assert.Equal(3m, result.Value.LatestReading!.Temperature);
        Assert.False(result.Value.LatestReading.IsValid);
    }

    [Fact]
    public async Task GetStatus_UsesReceiptTimeForCommunicationAndCaptureTimeForLatestMeasurement()
    {
        var exactlySixHours = Reading(8m, valid: true, capturedAt: Now.AddHours(-12), receivedAt: Now.AddHours(-6));
        var connected = await Service([Snapshot()], [exactlySixHours]).Handle(
            new GetWaterTroughStatusQuery(RanchId, TroughId), CancellationToken.None);
        var overSixHours = Reading(8m, valid: true, capturedAt: Now.AddHours(-12),
            receivedAt: Now.AddHours(-6).AddTicks(-1));
        var disconnected = await Service([Snapshot()], [overSixHours]).Handle(
            new GetWaterTroughStatusQuery(RanchId, TroughId), CancellationToken.None);

        Assert.Equal(WaterControllerStatus.Communicating, connected.Value!.ControllerStatus);
        Assert.Equal(exactlySixHours.CapturedAt, connected.Value.LatestReading!.CapturedAt);
        Assert.Equal(exactlySixHours.ReceivedAt, connected.Value.LastCommunicationAt);
        Assert.Equal(WaterControllerStatus.NoCommunication, disconnected.Value!.ControllerStatus);
    }

    [Theory]
    [InlineData(5, "BelowRange")]
    [InlineData(10, "WithinRange")]
    [InlineData(20, "WithinRange")]
    [InlineData(20.1, "AboveRange")]
    public async Task GetStatus_ComparesValidTemperatureToInclusiveConfiguredRange(double temperature,
        string expectedRangeStatus)
    {
        var reading = Reading((decimal)temperature, valid: true, capturedAt: Now.AddMinutes(-1), receivedAt: Now.AddMinutes(-1));

        var result = await Service([Snapshot()], [reading]).Handle(new GetWaterTroughStatusQuery(RanchId, TroughId),
            CancellationToken.None);

        Assert.Equal(expectedRangeStatus, result.Value!.RangeStatus.ToString());
    }

    [Fact]
    public async Task GetStatuses_DeniesAccountWithoutRanchReadPermission()
    {
        var access = new FakeAccess { IsAllowed = false };
        var service = Service([Snapshot()], [], access);

        var result = await service.Handle(new GetWaterTroughStatusesQuery(RanchId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("OperationsMonitoring.Forbidden", result.DomainError.Code);
    }

    private static WaterTroughMonitoringQueryService Service(WaterTroughMonitoringSnapshot[] snapshots,
        WaterTroughReading[] records, FakeAccess? access = null) => new(access ?? new FakeAccess(),
        new FakeIoTAssetsFacade(snapshots), new FakeReadingRepository(records),
        Options.Create(new OperationsMonitoringOptions()), new FakeTimeProvider(Now));

    private static WaterTroughMonitoringSnapshot Snapshot(bool hasController = true) =>
        new(TroughId, RanchId, "North trough", null, 10m, 20m, hasController ? ControllerId : null,
            hasController ? Now.AddDays(-1) : null);

    private static WaterTroughReading Reading(decimal temperature, bool valid, DateTimeOffset capturedAt,
        DateTimeOffset receivedAt)
    {
        var command = new RegisterWaterControllerReadingCommand(GatewayId, RanchId, ControllerId, TroughId,
            Guid.NewGuid(), temperature, WaterTemperatureUnit.Celsius, valid, WaterHeaterState.On, capturedAt,
            receivedAt);
        return WaterTroughReading.Create(command).Value!;
    }

    private sealed class FakeAccess : IMonitoringAccessControl
    {
        public bool IsAllowed { get; init; } = true;

        public Task<Guid?> GetCurrentAccountIdAsync(CancellationToken cancellationToken) => Task.FromResult<Guid?>(AccountId);

        public Task<bool> CanAccessRanchAsync(Guid accountId, Guid ranchId, MonitoringPermission permission,
            CancellationToken cancellationToken) => Task.FromResult(IsAllowed && accountId == AccountId && ranchId == RanchId);
    }

    private sealed class FakeIoTAssetsFacade(WaterTroughMonitoringSnapshot[] snapshots) : IIoTAssetsContextFacade
    {
        public Task<Result<IReadOnlyList<WaterTroughMonitoringSnapshot>>> GetWaterTroughMonitoringSnapshotsAsync(Guid ranchId,
            CancellationToken cancellationToken) => Task.FromResult(Result<IReadOnlyList<WaterTroughMonitoringSnapshot>>.Success(
            snapshots.Where(snapshot => snapshot.RanchId == ranchId).ToArray()));

        public Task<WaterControllerAssignmentSnapshot?> FindWaterControllerAssignmentAsync(Guid controllerId,
            DateTimeOffset instant, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Result<DeviceAssignmentSnapshot?>> GetCurrentCollarAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<Result<DeviceAssignmentSnapshot?>> ResolveAssignmentAtAsync(Guid ranchId, Guid deviceId, DateTimeOffset instant, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<Result<IReadOnlyList<DeviceAssignmentSnapshot>>> GetActiveAssignmentsAsync(Guid ranchId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<Result<WaterTroughConfiguration>> GetWaterTroughConfigurationAsync(Guid ranchId, Guid waterTroughId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<Result> RegisterCommunicationAsync(Guid ranchId, Guid deviceId, DateTimeOffset communicatedAt, DateTimeOffset receivedAt, int? batteryPercentage, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<Result<int>> ReleaseCollarForInactiveCattleAsync(Guid ranchId, Guid cattleId, DateTimeOffset deactivatedAt, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<EdgeGatewaySnapshot?> AuthenticateEdgeGatewayAsync(string presentedKey, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<DeviceAttributionSnapshot?> FindDeviceAttributionAsync(Guid deviceId, DateTimeOffset instant, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<DeviceAssignmentSnapshot>> FindOpenCollarAssignmentsAsync(Guid ranchId, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class FakeReadingRepository(WaterTroughReading[] records) : IWaterTroughReadingRepository
    {
        public Task<IReadOnlyList<WaterTroughReading>> FindLatestByRanchAsync(Guid ranchId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<WaterTroughReading>>(records.Where(item => item.RanchId == ranchId)
                .GroupBy(item => new { item.ControllerId, item.WaterTroughId })
                .Select(group => group.OrderByDescending(item => item.CapturedAt).First()).ToArray());

        public Task<IReadOnlyList<WaterTroughReading>> FindLatestReceivedByRanchAsync(Guid ranchId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<WaterTroughReading>>(records.Where(item => item.RanchId == ranchId)
                .GroupBy(item => new { item.ControllerId, item.WaterTroughId })
                .Select(group => group.OrderByDescending(item => item.ReceivedAt).First()).ToArray());

        public Task AddAsync(WaterTroughReading reading, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<WaterTroughReading?> FindByControllerAndReadingIdAsync(Guid controllerId, Guid readingId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<WaterTroughReading>> FindByControllerAsync(Guid ranchId, Guid controllerId, TimePeriod period, int skip, int take, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<int> CountByControllerAsync(Guid ranchId, Guid controllerId, TimePeriod period, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
