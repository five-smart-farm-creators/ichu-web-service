using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;

/// <summary>
///     Application query service interface for handling monitoring threshold read operations.
/// </summary>
public interface IMonitoringThresholdQueryService
{
    /// <summary>
    ///     Handles the listing of the thresholds of an authorized ranch asynchronously.
    /// </summary>
    /// <param name="query">The query containing the ranch identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the thresholds on success, or the domain error on failure.</returns>
    Task<Result<IReadOnlyList<MonitoringThreshold>>> Handle(GetThresholdsByRanchQuery query, CancellationToken cancellationToken);
}
