using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Services;

/// <summary>
///     Application query service interface for publishing the monitoring rules of a ranch to Edge Gateways.
/// </summary>
public interface IMonitoringRuleSetQueryService
{
    /// <summary>
    ///     Handles the retrieval of the rules of the gateway's ranch asynchronously.
    /// </summary>
    /// <param name="query">The query containing the gateway and the revision it already holds.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A result containing the rules on success, or an error enum on failure, including when the gateway already
    ///     holds the current revision.
    /// </returns>
    Task<Result<MonitoringRuleSnapshot, EGetEdgeMonitoringRulesError>> Handle(GetEdgeMonitoringRulesQuery query, CancellationToken cancellationToken);
}
