using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;

public interface IEdgeSynchronizationCommandService
{
    /// <summary>
    ///     Decides every entry of a batch on its own, so that one rejected entry never blocks the others.
    /// </summary>
    Task<Result<IReadOnlyList<SyncEntryResult>>> Handle(SynchronizeEdgeBatchCommand command, CancellationToken cancellationToken);
}
