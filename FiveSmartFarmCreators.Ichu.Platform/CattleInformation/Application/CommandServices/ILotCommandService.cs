using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.CommandServices;

/// <summary>
///     Coordinates lot changes after checking the caller's ranch management permission.
/// </summary>
public interface ILotCommandService
{
    Task<Result<Lot>> HandleAsync(CreateLotCommand command, CancellationToken cancellationToken);

    Task<Result<Lot>> HandleAsync(AssignCattleToLotCommand command, CancellationToken cancellationToken);

    Task<Result<Lot>> HandleAsync(RemoveCattleFromLotCommand command, CancellationToken cancellationToken);
}
