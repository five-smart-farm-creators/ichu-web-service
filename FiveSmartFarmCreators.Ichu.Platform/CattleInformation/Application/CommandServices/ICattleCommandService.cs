using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.CommandServices;

/// <summary>
///     Coordinates animal changes after checking the caller's ranch permission.
/// </summary>
public interface ICattleCommandService
{
    Task<Result<Cattle>> HandleAsync(RegisterCattleCommand command, CancellationToken cancellationToken);

    Task<Result<Cattle>> HandleAsync(UpdateCattleCommand command, CancellationToken cancellationToken);

    Task<Result<Cattle>> HandleAsync(ChangeLifeStageCommand command, CancellationToken cancellationToken);

    Task<Result<Cattle>> HandleAsync(DeactivateCattleCommand command, CancellationToken cancellationToken);
}
