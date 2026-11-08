using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.IoTAssets.Domain;

public class IoTDeviceTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid CattleId = Guid.Parse("33333333-3333-4333-8333-333333333333");
    private static readonly DateTimeOffset At = new(2026, 11, 7, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Register_CreatesAnAvailableDeviceWithATrimmedSerial()
    {
        var device = IoTDevice.Register(new RegisterDeviceCommand(RanchId, "  COL-001  ", DeviceType.SmartCollar, null), At).Value!;

        Assert.Equal("COL-001", device.Serial.Value);
        Assert.Equal(DeviceStatus.Available, device.Status);
        Assert.True(device.IsAvailable());
    }

    [Fact]
    public void Register_ReturnsABusinessErrorForABlankSerial()
    {
        var result = IoTDevice.Register(new RegisterDeviceCommand(RanchId, " ", DeviceType.SmartCollar, null), At);

        Assert.True(result.IsFailure);
        Assert.Equal(IoTAssetsErrors.InvalidInput.Code, result.DomainError.Code);
    }

    [Fact]
    public void AssignTo_ASecondTargetWhileTheDeviceIsInServiceIsRefused()
    {
        var device = AvailableCollar();
        device.AssignTo(CattleId, At);

        var result = device.AssignTo(Guid.NewGuid(), At.AddMinutes(1));

        Assert.Equal(IoTAssetsErrors.InvalidState.Code, result.DomainError.Code);
        Assert.Single(device.Assignments);
    }

    [Fact]
    public void Release_ClosesTheOpenAssignmentWithItsReasonAndFreesTheDevice()
    {
        var device = AvailableCollar();
        device.AssignTo(CattleId, At);

        var result = device.Release("Cattle sold", At.AddHours(2));

        Assert.True(result.IsSuccess);
        Assert.Equal(DeviceStatus.Available, device.Status);
        Assert.Equal("Cattle sold", device.Assignments.Single().ReleaseReason);
        Assert.Equal(At.AddHours(2), device.Assignments.Single().EndedAt);
    }

    [Fact]
    public void EnsureVersion_WithAStaleVersionIsAConflict()
    {
        var device = AvailableCollar();

        var result = device.EnsureVersion(Guid.NewGuid());

        Assert.Equal(IoTAssetsErrors.Conflict.Code, result.DomainError.Code);
    }

    [Fact]
    public void TemperatureRange_Create_RejectsAMinimumAboveTheMaximum()
    {
        var result = TemperatureRange.Create(30m, 20m);

        Assert.Equal(IoTAssetsErrors.InvalidInput.Code, result.DomainError.Code);
    }

    private static IoTDevice AvailableCollar() =>
        IoTDevice.Register(new RegisterDeviceCommand(RanchId, "COL-001", DeviceType.SmartCollar, null), At).Value!;
}
