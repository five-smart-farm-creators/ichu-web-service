using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Services;

/// <summary>
///     Application command service interface for handling telemetry reading write operations.
/// </summary>
public interface ITelemetryReadingCommandService
{
    /// <summary>
    ///     Handles the synchronization of one reading relayed by an authenticated Edge Gateway asynchronously.
    /// </summary>
    /// <param name="command">The command containing the reading and the gateway that relayed it.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The individual result of the reading: accepted, duplicate, rejected or conflicting.</returns>
    Task<SyncEntryResult> Handle(SynchronizeReadingCommand command, CancellationToken cancellationToken);
}
