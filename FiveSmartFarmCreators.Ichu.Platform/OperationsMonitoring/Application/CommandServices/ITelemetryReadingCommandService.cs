using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;

public interface ITelemetryReadingCommandService
{
    /// <summary>
    ///     Synchronizes one reading relayed by an authenticated Edge Gateway.
    /// </summary>
    Task<SyncEntryResult> Handle(SynchronizeReadingCommand command, CancellationToken cancellationToken);
}
