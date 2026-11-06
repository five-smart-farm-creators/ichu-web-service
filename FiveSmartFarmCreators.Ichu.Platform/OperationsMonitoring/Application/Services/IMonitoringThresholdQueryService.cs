using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Services;

/// <summary>
///     Application query service interface for handling monitoring threshold read operations.
/// </summary>
public interface IMonitoringThresholdQueryService
{
    /// <summary>
    ///     Handles the listing of the thresholds of an authorized ranch asynchronously.
    /// </summary>
    /// <param name="query">The query containing the ranch identifier.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the thresholds on success, or an error enum on failure.</returns>
    Task<Result<IReadOnlyList<MonitoringThreshold>, EGetThresholdsError>> Handle(GetThresholdsByRanchQuery query, CancellationToken cancellationToken);
}
