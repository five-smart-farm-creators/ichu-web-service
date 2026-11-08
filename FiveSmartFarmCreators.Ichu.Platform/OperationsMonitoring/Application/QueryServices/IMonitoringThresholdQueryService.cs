using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;

public interface IMonitoringThresholdQueryService
{
    Task<Result<IReadOnlyList<MonitoringThreshold>>> Handle(GetThresholdsByRanchQuery query, CancellationToken cancellationToken);
}
