using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;

public interface IWaterTroughRepository
{
    Task AddAsync(WaterTrough waterTrough, CancellationToken cancellationToken);
    Task<WaterTrough?> FindByIdAsync(Guid ranchId, Guid waterTroughId, CancellationToken cancellationToken);
    Task<WaterTrough?> FindByIdAsNoTrackingAsync(Guid ranchId, Guid waterTroughId, CancellationToken cancellationToken);
    Task<IReadOnlyList<WaterTrough>> FindByRanchAsNoTrackingAsync(Guid ranchId, CancellationToken cancellationToken);
}
