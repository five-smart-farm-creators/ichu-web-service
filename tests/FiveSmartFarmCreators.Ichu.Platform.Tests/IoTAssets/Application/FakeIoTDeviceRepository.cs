using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.IoTAssets.Application;

public sealed class FakeIoTDeviceRepository : IIoTDeviceRepository
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

    public Task<IoTDevice?> FindByIdAsNoTrackingAsync(DeviceId deviceId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.SingleOrDefault(device => device.Id == deviceId));

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
