using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class IoTDeviceRepository(AppDbContext context) : IIoTDeviceRepository
{
    public async Task AddAsync(IoTDevice device, CancellationToken cancellationToken)
    {
        await context.Set<IoTDevice>().AddAsync(device, cancellationToken);
    }

    public Task<IoTDevice?> FindByIdAsync(Guid ranchId, DeviceId deviceId, CancellationToken cancellationToken)
    {
        return Devices().FirstOrDefaultAsync(d => d.RanchId == ranchId && d.Id == deviceId, cancellationToken);
    }

    public Task<IoTDevice?> FindByIdAsNoTrackingAsync(Guid ranchId, DeviceId deviceId, CancellationToken cancellationToken)
    {
        return Devices().AsNoTracking().FirstOrDefaultAsync(d => d.RanchId == ranchId && d.Id == deviceId, cancellationToken);
    }

    public Task<IoTDevice?> FindByIdAsNoTrackingAsync(DeviceId deviceId, CancellationToken cancellationToken)
    {
        return Devices().AsNoTracking().FirstOrDefaultAsync(d => d.Id == deviceId, cancellationToken);
    }

    public async Task<IReadOnlyList<IoTDevice>> FindByRanchAsNoTrackingAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        return await Devices().AsNoTracking().Where(d => d.RanchId == ranchId)
            .OrderBy(d => d.Serial.Value).ThenBy(d => d.Id).ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsBySerialAsync(DeviceSerial serial, CancellationToken cancellationToken)
    {
        return context.Set<IoTDevice>().AnyAsync(d => d.Serial.Value == serial.Value, cancellationToken);
    }

    public Task<bool> HasOpenAssignmentAsync(Guid ranchId, Guid targetId, AssignmentTargetType targetType,
        CancellationToken cancellationToken)
    {
        return context.Set<IoTDevice>().AnyAsync(d => d.RanchId == ranchId
            && d.Assignments.Any(a => a.TargetId == targetId && a.TargetType == targetType && a.EndedAt == null),
            cancellationToken);
    }

    public Task<int> CountAssignedCollarsAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        return context.Set<IoTDevice>().CountAsync(d => d.RanchId == ranchId && d.Type == DeviceType.SmartCollar
            && d.Assignments.Any(a => a.EndedAt == null), cancellationToken);
    }

    public Task<IoTDevice?> FindByAssignedCattleAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken)
    {
        return AssignedToCattle(ranchId, cattleId).SingleOrDefaultAsync(cancellationToken);
    }

    public Task<IoTDevice?> FindByAssignedCattleAsNoTrackingAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken)
    {
        return AssignedToCattle(ranchId, cattleId).AsNoTracking().SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<IoTDevice>> FindAssignedCollarsAsNoTrackingAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        return await Devices().AsNoTracking()
            .Where(d => d.RanchId == ranchId && d.Type == DeviceType.SmartCollar && d.Assignments.Any(a => a.EndedAt == null))
            .ToListAsync(cancellationToken);
    }

    private IQueryable<IoTDevice> Devices() =>
        context.Set<IoTDevice>().Include(d => d.Assignments).AsSplitQuery();

    private IQueryable<IoTDevice> AssignedToCattle(Guid ranchId, Guid cattleId) =>
        Devices().Where(d => d.RanchId == ranchId && d.Type == DeviceType.SmartCollar
            && d.Assignments.Any(a => a.TargetId == cattleId && a.TargetType == AssignmentTargetType.Cattle && a.EndedAt == null));
}
