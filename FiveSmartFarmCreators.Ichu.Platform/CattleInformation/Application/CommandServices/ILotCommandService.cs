using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.CommandServices;

/// <summary>
/// Coordinates lot changes after checking the current caller's ranch management permission.
/// </summary>
public interface ILotCommandService
{
    /// <summary>
    /// Creates a management group within the authorized ranch.
    /// </summary>
    /// <returns>The persisted lot with its generated identifier and version.</returns>
    Task<Lot> HandleAsync(CreateLotCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Assigns active cattle from the same ranch and saves both concurrency guards together.
    /// </summary>
    /// <returns>The lot with the membership; a current duplicate membership is unchanged.</returns>
    Task<Lot> HandleAsync(AssignCattleToLotCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Removes membership using the expected lot version without deleting the animal.
    /// </summary>
    /// <returns>The lot after removal, or the unchanged lot when no such membership exists.</returns>
    Task<Lot> HandleAsync(RemoveCattleFromLotCommand command, CancellationToken cancellationToken);
}
