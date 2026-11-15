using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;

public interface IWaterTroughMonitoringQueryService
{
    Task<Result<PagedResult<Domain.Model.Aggregates.WaterTroughReading>>> Handle(GetWaterControllerReadingsQuery query,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<WaterTroughStatus>>> Handle(GetWaterTroughStatusesQuery query,
        CancellationToken cancellationToken);

    Task<Result<WaterTroughStatus>> Handle(GetWaterTroughStatusQuery query, CancellationToken cancellationToken);
}
