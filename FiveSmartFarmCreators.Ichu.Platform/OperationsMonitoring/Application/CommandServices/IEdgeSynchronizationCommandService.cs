using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;

/// <summary>
///     Application command service interface for synchronizing the batches sent by Edge Gateways.
/// </summary>
public interface IEdgeSynchronizationCommandService
{
    /// <summary>
    ///     Handles the synchronization of a batch asynchronously. Every entry is evaluated and saved independently so
    ///     that one rejected entry never blocks the others.
    /// </summary>
    /// <param name="command">The command containing the readings, alerts and alert changes.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A result containing one result per entry on success, or the domain error on failure.</returns>
    Task<Result<IReadOnlyList<SyncEntryResult>>> Handle(SynchronizeEdgeBatchCommand command, CancellationToken cancellationToken);
}
