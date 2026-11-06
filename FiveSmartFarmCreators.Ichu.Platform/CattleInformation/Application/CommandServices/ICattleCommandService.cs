using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.CommandServices;

public interface ICattleCommandService
{
    Task<Cattle> HandleAsync(RegisterCattleCommand command, CancellationToken cancellationToken);
    Task<Cattle> HandleAsync(UpdateCattleCommand command, CancellationToken cancellationToken);
    Task<Cattle> HandleAsync(ChangeLifeStageCommand command, CancellationToken cancellationToken);
    Task<Cattle> HandleAsync(DeactivateCattleCommand command, CancellationToken cancellationToken);
}
