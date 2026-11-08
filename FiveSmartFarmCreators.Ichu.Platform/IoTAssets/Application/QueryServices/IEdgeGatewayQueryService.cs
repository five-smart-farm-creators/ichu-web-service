using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.QueryServices;

public interface IEdgeGatewayQueryService
{
    Task<Result<EdgeGateway>> HandleAsync(GetEdgeGatewayByIdQuery query, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<EdgeGateway>>> HandleAsync(GetEdgeGatewaysByRanchQuery query, CancellationToken cancellationToken);
}
