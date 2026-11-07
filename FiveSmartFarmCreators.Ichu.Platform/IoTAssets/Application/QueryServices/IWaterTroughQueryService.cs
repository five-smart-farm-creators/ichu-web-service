using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.QueryServices;

public interface IWaterTroughQueryService
{
    Task<Result<WaterTrough>> HandleAsync(GetWaterTroughByIdQuery query, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<WaterTrough>>> HandleAsync(GetWaterTroughsQuery query, CancellationToken cancellationToken);
}
