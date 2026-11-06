using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.CommandServices;

/// <summary>
/// Coordinates authorized membership changes while sharing the animal and lot persistence transaction.
/// </summary>
public sealed class LotCommandService(
    ILotRepository lotRepository,
    ICattleRepository cattleRepository,
    ICattleUnitOfWork unitOfWork,
    ICattleAccessService accessService,
    TimeProvider timeProvider) : ILotCommandService
{
    /// <inheritdoc />
    public async Task<Lot> HandleAsync(CreateLotCommand command, CancellationToken cancellationToken)
    {
        await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        var lot = new Lot(command);
        await lotRepository.AddAsync(lot, cancellationToken);
        await unitOfWork.CommitAsync(null, cancellationToken);
        return lot;
    }

    /// <inheritdoc />
    public async Task<Lot> HandleAsync(AssignCattleToLotCommand command, CancellationToken cancellationToken)
    {
        await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        var lot = await FindAsync(command.RanchId, command.LotId, cancellationToken);
        if (command.CattleId == Guid.Empty)
        {
            throw new CattleInformationException("invalid_input", "A cattle identifier is required.");
        }

        var cattle = await cattleRepository.FindByIdAsync(command.RanchId, command.CattleId, cancellationToken)
            ?? throw new CattleInformationException("not_found", "The animal was not found in this ranch.");
        lot.AddCattle(cattle, command.ExpectedVersion, timeProvider.GetUtcNow());
        await unitOfWork.CommitAsync(cattle, cancellationToken);
        return lot;
    }

    /// <inheritdoc />
    public async Task<Lot> HandleAsync(RemoveCattleFromLotCommand command, CancellationToken cancellationToken)
    {
        await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        var lot = await FindAsync(command.RanchId, command.LotId, cancellationToken);
        lot.RemoveCattle(command.CattleId, command.ExpectedVersion);
        await unitOfWork.CommitAsync(null, cancellationToken);
        return lot;
    }

    private async Task<Lot> FindAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken)
    {
        if (ranchId == Guid.Empty || lotId == Guid.Empty)
        {
            throw new CattleInformationException("invalid_input", "Ranch and lot identifiers are required.");
        }

        return await lotRepository.FindByIdAsync(ranchId, lotId, cancellationToken)
            ?? throw new CattleInformationException("not_found", "The lot was not found in this ranch.");
    }
}
