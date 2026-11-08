using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;

public interface IIoTDeviceRepository
{
    Task AddAsync(IoTDevice device, CancellationToken cancellationToken);
    Task<IoTDevice?> FindByIdAsync(Guid ranchId, DeviceId deviceId, CancellationToken cancellationToken);
    Task<IoTDevice?> FindByIdAsNoTrackingAsync(Guid ranchId, DeviceId deviceId, CancellationToken cancellationToken);
    Task<IoTDevice?> FindByIdAsNoTrackingAsync(DeviceId deviceId, CancellationToken cancellationToken);
    Task<IReadOnlyList<IoTDevice>> FindByRanchAsNoTrackingAsync(Guid ranchId, CancellationToken cancellationToken);
    Task<bool> ExistsBySerialAsync(DeviceSerial serial, CancellationToken cancellationToken);
    Task<bool> HasOpenAssignmentAsync(Guid ranchId, Guid targetId, AssignmentTargetType targetType, CancellationToken cancellationToken);
    Task<int> CountAssignedCollarsAsync(Guid ranchId, CancellationToken cancellationToken);
    Task<IoTDevice?> FindByAssignedCattleAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken);
    Task<IoTDevice?> FindByAssignedCattleAsNoTrackingAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken);
}
