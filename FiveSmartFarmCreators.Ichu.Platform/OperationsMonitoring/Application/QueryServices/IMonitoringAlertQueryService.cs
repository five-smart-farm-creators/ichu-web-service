using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;

public interface IMonitoringAlertQueryService
{
    Task<Result<PagedResult<MonitoringAlert>>> Handle(GetAlertsQuery query, CancellationToken cancellationToken);

    Task<Result<MonitoringAlert>> Handle(GetAlertByIdQuery query, CancellationToken cancellationToken);
}
