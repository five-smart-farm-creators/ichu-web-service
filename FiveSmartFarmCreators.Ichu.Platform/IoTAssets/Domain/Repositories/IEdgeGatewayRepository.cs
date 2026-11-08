using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;

public interface IEdgeGatewayRepository
{
    Task AddAsync(EdgeGateway gateway, CancellationToken cancellationToken);
    Task<EdgeGateway?> FindByIdAsync(Guid ranchId, EdgeGatewayId gatewayId, CancellationToken cancellationToken);
    Task<EdgeGateway?> FindByIdAsNoTrackingAsync(Guid ranchId, EdgeGatewayId gatewayId, CancellationToken cancellationToken);
    Task<EdgeGateway?> FindByIdAsNoTrackingAsync(EdgeGatewayId gatewayId, CancellationToken cancellationToken);
    Task<IReadOnlyList<EdgeGateway>> FindByRanchAsNoTrackingAsync(Guid ranchId, CancellationToken cancellationToken);
}
