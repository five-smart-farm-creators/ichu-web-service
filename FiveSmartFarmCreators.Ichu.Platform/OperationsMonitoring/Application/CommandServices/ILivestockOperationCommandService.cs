using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;

/// <summary>
///     Application command service interface for handling livestock operation write operations.
/// </summary>
public interface ILivestockOperationCommandService
{
    /// <summary>
    ///     Handles the registration of a field event or clinical intervention asynchronously. Repeating the same
    ///     operation returns the stored record.
    /// </summary>
    /// <param name="command">The command containing the operation data.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the registration on success, or the domain error on failure.</returns>
    /// <exception cref="ArgumentException">Thrown when a required field is missing or a value violates a domain rule.</exception>
    Task<Result<LivestockOperationRegistration>> Handle(RegisterLivestockOperationCommand command, CancellationToken cancellationToken);

    /// <summary>
    ///     Handles the synchronization of the operations recorded offline by the mobile application asynchronously.
    /// </summary>
    /// <param name="command">The command containing the recorded operations.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A result containing one result per operation on success, or the domain error on failure.</returns>
    Task<Result<IReadOnlyList<SyncEntryResult>>> Handle(SynchronizeMobileBatchCommand command, CancellationToken cancellationToken);
}
