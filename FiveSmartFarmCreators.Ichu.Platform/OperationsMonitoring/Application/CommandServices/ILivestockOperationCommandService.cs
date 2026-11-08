using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;

public interface ILivestockOperationCommandService
{
    /// <summary>
    ///     Registers a field event or a clinical intervention. Repeating the same operation returns the stored record.
    /// </summary>
    Task<Result<LivestockOperationRegistration>> Handle(RegisterLivestockOperationCommand command, CancellationToken cancellationToken);

    /// <summary>
    ///     Synchronizes the operations that the mobile application recorded offline, with one result per operation.
    /// </summary>
    Task<Result<IReadOnlyList<SyncEntryResult>>> Handle(SynchronizeMobileBatchCommand command, CancellationToken cancellationToken);
}
