using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;

/// <summary>
///     Application command service interface for handling monitoring alert write operations.
/// </summary>
public interface IMonitoringAlertCommandService
{
    /// <summary>
    ///     Handles the attention of a pending alert through the central API asynchronously. The response time uses the
    ///     server time of the request. Repeating the same operation returns the stored alert.
    /// </summary>
    /// <param name="command">The command containing the attention data.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the stored alert on success, or an error enum on failure.</returns>
    /// <exception cref="ArgumentException">Thrown when the attention data violates a domain rule.</exception>
    Task<Result<MonitoringAlert, EAcknowledgeAlertError>> Handle(AcknowledgeAlertCommand command, CancellationToken cancellationToken);

    /// <summary>
    ///     Handles the synchronization of one alert evaluated by an authenticated Edge Gateway asynchronously. New
    ///     evidence of the same condition within the deduplication window joins the open alert.
    /// </summary>
    /// <param name="command">The command containing the alert and the gateway that evaluated it.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The individual result of the alert: accepted, duplicate, rejected or conflicting.</returns>
    Task<SyncEntryResult> Handle(SynchronizeAlertCommand command, CancellationToken cancellationToken);

    /// <summary>
    ///     Handles the synchronization of one change to an existing alert asynchronously.
    /// </summary>
    /// <param name="command">The command containing the change and the gateway that sent it.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The individual result of the change: accepted, duplicate, rejected or conflicting.</returns>
    Task<SyncEntryResult> Handle(SynchronizeAlertChangeCommand command, CancellationToken cancellationToken);
}
