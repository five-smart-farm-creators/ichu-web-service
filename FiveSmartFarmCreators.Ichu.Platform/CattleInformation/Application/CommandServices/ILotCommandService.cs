using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.CommandServices;

public interface ILotCommandService
{
    Task<Lot> HandleAsync(CreateLotCommand command, CancellationToken cancellationToken);
    Task<Lot> HandleAsync(AssignCattleToLotCommand command, CancellationToken cancellationToken);
    Task<Lot> HandleAsync(RemoveCattleFromLotCommand command, CancellationToken cancellationToken);
}
