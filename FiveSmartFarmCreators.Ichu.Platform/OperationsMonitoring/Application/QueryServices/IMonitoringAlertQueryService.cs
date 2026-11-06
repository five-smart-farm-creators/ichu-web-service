using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
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
    /// <returns>A result containing the page of alerts on success, or the domain error on failure.</returns>
    Task<Result<PagedResult<MonitoringAlert>>> Handle(GetAlertsQuery query, CancellationToken cancellationToken);

    /// <summary>
    ///     Handles the retrieval of an alert of an authorized ranch asynchronously.
    /// </summary>
    /// <param name="query">The query containing the alert identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the alert on success, or the domain error on failure.</returns>
    Task<Result<MonitoringAlert>> Handle(GetAlertByIdQuery query, CancellationToken cancellationToken);
}
