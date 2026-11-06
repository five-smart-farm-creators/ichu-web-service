using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.CommandServices;

/// <summary>
/// Coordinates animal changes after checking the current caller's ranch management permission.
/// </summary>
public interface ICattleCommandService
{
    /// <summary>
    /// Registers active cattle after checking ear-tag uniqueness within the authorized ranch.
    /// </summary>
    /// <returns>The animal after its registration and pending event have been persisted.</returns>
    Task<Cattle> HandleAsync(RegisterCattleCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Corrects an active animal's details using its expected version while preserving stage history.
    /// </summary>
    /// <returns>The persisted animal with its new concurrency version.</returns>
    Task<Cattle> HandleAsync(UpdateCattleCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Changes an active animal's stage and persists its history and pending lifecycle event together.
    /// </summary>
    /// <returns>The persisted animal with its updated stage history and version.</returns>
    Task<Cattle> HandleAsync(ChangeLifeStageCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Persists an animal's exit and queues its lifecycle event for downstream handling.
    /// </summary>
    /// <returns>The inactive animal; completion does not confirm downstream collar release.</returns>
    Task<Cattle> HandleAsync(DeactivateCattleCommand command, CancellationToken cancellationToken);
}
