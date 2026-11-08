using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;

public interface IMonitoringRuleSetQueryService
{
    /// <summary>
    ///     Publishes the rules of the gateway's ranch when the gateway holds an older revision. A gateway that holds the current revision receives the RulesNotModified outcome instead.
    /// </summary>
    Task<Result<MonitoringRuleSnapshot>> Handle(GetEdgeMonitoringRulesQuery query, CancellationToken cancellationToken);
}
