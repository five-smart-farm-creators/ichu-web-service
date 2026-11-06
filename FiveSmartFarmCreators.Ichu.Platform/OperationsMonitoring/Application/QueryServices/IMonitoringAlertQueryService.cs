using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;

/// <summary>
///     Application query service interface for handling monitoring alert read operations.
/// </summary>
public interface IMonitoringAlertQueryService
{
    /// <summary>
    ///     Handles the listing of the alerts of an authorized ranch asynchronously.
    /// </summary>
    /// <param name="query">The query containing the filters and the page.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the page of alerts on success, or an error enum on failure.</returns>
    Task<Result<PagedResult<MonitoringAlert>, EGetAlertsError>> Handle(GetAlertsQuery query, CancellationToken cancellationToken);

    /// <summary>
    ///     Handles the retrieval of an alert of an authorized ranch asynchronously.
    /// </summary>
    /// <param name="query">The query containing the alert identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the alert on success, or an error enum on failure.</returns>
    Task<Result<MonitoringAlert, EGetAlertByIdError>> Handle(GetAlertByIdQuery query, CancellationToken cancellationToken);
}
