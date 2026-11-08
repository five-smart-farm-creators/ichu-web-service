using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;

public interface IMonitoringAlertCommandService
{
    /// <summary>
    ///     Attends a pending alert at the server time of the request. Repeating the same operation returns the stored alert.
    /// </summary>
    Task<Result<MonitoringAlert>> Handle(AcknowledgeAlertCommand command, CancellationToken cancellationToken);

    /// <summary>
    ///     Synchronizes one alert evaluated by an Edge Gateway. New evidence of the same condition within the deduplication window joins the open alert.
    /// </summary>
    Task<SyncEntryResult> Handle(SynchronizeAlertCommand command, CancellationToken cancellationToken);

    Task<SyncEntryResult> Handle(SynchronizeAlertChangeCommand command, CancellationToken cancellationToken);
}
