using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;

public interface IWaterTroughRepository
{
    Task AddAsync(WaterTrough waterTrough, CancellationToken cancellationToken);
    Task<WaterTrough?> FindByIdAsync(Guid ranchId, WaterTroughId waterTroughId, CancellationToken cancellationToken);
    Task<WaterTrough?> FindByIdAsNoTrackingAsync(Guid ranchId, WaterTroughId waterTroughId, CancellationToken cancellationToken);
    Task<IReadOnlyList<WaterTrough>> FindByRanchAsNoTrackingAsync(Guid ranchId, CancellationToken cancellationToken);
}
