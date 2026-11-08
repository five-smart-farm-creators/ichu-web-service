using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;

public interface ILivestockOperationQueryService
{
    Task<Result<IReadOnlyList<LivestockOperation>>> Handle(GetLivestockOperationsQuery query, CancellationToken cancellationToken);
}
