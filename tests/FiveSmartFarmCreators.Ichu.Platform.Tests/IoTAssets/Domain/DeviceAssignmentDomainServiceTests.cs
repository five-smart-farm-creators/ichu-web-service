using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Services;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.IoTAssets.Domain;

public class DeviceAssignmentDomainServiceTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly DateTimeOffset At = new(2026, 11, 7, 10, 0, 0, TimeSpan.Zero);
    private readonly DeviceAssignmentDomainService _service = new();

    [Fact]
    public void Assign_ACollarWithoutAnActiveSubscriptionIsRefused()
    {
        var result = _service.Assign(Collar(), Guid.NewGuid(), 0, 10, subscriptionActive: false, At);

        Assert.Equal(IoTAssetsErrors.SubscriptionRequired.Code, result.DomainError.Code);
    }

    [Fact]
    public void Assign_ACollarWhenThePlanLimitIsReachedIsRefused()
    {
        var result = _service.Assign(Collar(), Guid.NewGuid(), assignedCollars: 10, maximumCollars: 10, subscriptionActive: true, At);

        Assert.Equal(IoTAssetsErrors.DeviceLimitReached.Code, result.DomainError.Code);
    }

    [Fact]
    public void Assign_AWaterControllerIsNotCountedAgainstTheCollarLimit()
    {
        var controller = IoTDevice.Register(new RegisterDeviceCommand(RanchId, "WC-001", DeviceType.WaterController, null), At).Value!;

        var result = _service.Assign(controller, Guid.NewGuid(), assignedCollars: 10, maximumCollars: 10, subscriptionActive: false, At);

        Assert.True(result.IsSuccess);
        Assert.Equal(AssignmentTargetType.WaterTrough, result.Value!.TargetType);
    }

    private static IoTDevice Collar() =>
        IoTDevice.Register(new RegisterDeviceCommand(RanchId, "COL-001", DeviceType.SmartCollar, null), At).Value!;
}
