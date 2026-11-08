using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.CommandServices;

/// <summary>
///     Changes lot membership after checking access. The lot and the animal are saved in one unit of work, so both
///     concurrency guards apply together.
/// </summary>
public sealed class LotCommandService(
    ILotRepository lots,
    ICattleRepository cattles,
    IUnitOfWork unitOfWork,
    ICattleAccessService accessService,
    TimeProvider timeProvider) : ILotCommandService
{
    /// <inheritdoc />
    public async Task<Result<Lot>> HandleAsync(CreateLotCommand command, CancellationToken cancellationToken)
    {
        var access = await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<Lot>.Failure(access.DomainError);
        }

        var creation = Lot.Create(command);
        if (creation.IsFailure)
        {
            return creation;
        }

        await lots.AddAsync(creation.Value!, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return creation;
    }

    /// <inheritdoc />
    public async Task<Result<Lot>> HandleAsync(AssignCattleToLotCommand command, CancellationToken cancellationToken)
    {
        var access = await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<Lot>.Failure(access.DomainError);
        }

        var found = await FindLotAsync(command.RanchId, command.LotId, cancellationToken);
        if (found.IsFailure)
        {
            return found;
        }

        if (command.CattleId.Value == Guid.Empty)
        {
            return Result<Lot>.Failure(CattleInformationErrors.CattleIdentifierRequired);
        }

        var cattle = await cattles.FindByIdAsync(command.RanchId, command.CattleId, cancellationToken);
        if (cattle is null)
        {
            return Result<Lot>.Failure(CattleInformationErrors.CattleNotFound);
        }

        var assignment = found.Value!.AddCattle(cattle, command.ExpectedVersion, timeProvider.GetUtcNow());
        if (assignment.IsFailure)
        {
            return Result<Lot>.Failure(assignment.DomainError);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return found;
    }

    /// <inheritdoc />
    public async Task<Result<Lot>> HandleAsync(RemoveCattleFromLotCommand command, CancellationToken cancellationToken)
    {
        var access = await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<Lot>.Failure(access.DomainError);
        }

        var found = await FindLotAsync(command.RanchId, command.LotId, cancellationToken);
        if (found.IsFailure)
        {
            return found;
        }

        var removal = found.Value!.RemoveCattle(command.CattleId, command.ExpectedVersion);
        if (removal.IsFailure)
        {
            return Result<Lot>.Failure(removal.DomainError);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return found;
    }

    private async Task<Result<Lot>> FindLotAsync(Guid ranchId, LotId lotId, CancellationToken cancellationToken)
    {
        if (ranchId == Guid.Empty)
        {
            return Result<Lot>.Failure(CattleInformationErrors.RanchIdentifierRequired);
        }

        if (lotId.Value == Guid.Empty)
        {
            return Result<Lot>.Failure(CattleInformationErrors.LotIdentifierRequired);
        }

        var lot = await lots.FindByIdAsync(ranchId, lotId, cancellationToken);
        return lot is null
            ? Result<Lot>.Failure(CattleInformationErrors.LotNotFound)
            : Result<Lot>.Success(lot);
    }
}
