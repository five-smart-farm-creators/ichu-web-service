using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.CommandServices;

/// <summary>
///     Checks access and the ear tag rule here and leaves the lifecycle rules to the aggregate. A rejected change saves nothing.
/// </summary>
public sealed class CattleCommandService(
    ICattleRepository cattles,
    IUnitOfWork unitOfWork,
    ICattleAccessService accessService,
    TimeProvider timeProvider) : ICattleCommandService
{
    /// <inheritdoc />
    public async Task<Result<Cattle>> HandleAsync(RegisterCattleCommand command, CancellationToken cancellationToken)
    {
        var access = await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<Cattle>.Failure(access.DomainError);
        }

        var registration = Cattle.Register(command, Today());
        if (registration.IsFailure)
        {
            return registration;
        }

        var cattle = registration.Value!;
        var duplicate = await FindDuplicateEarTagAsync(cattle.RanchId, cattle.EarTag, cattle.Id, cancellationToken);
        if (duplicate is not null)
        {
            return Result<Cattle>.Failure(duplicate);
        }

        await cattles.AddAsync(cattle, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Cattle>.Success(cattle);
    }

    /// <inheritdoc />
    public async Task<Result<Cattle>> HandleAsync(UpdateCattleCommand command, CancellationToken cancellationToken)
    {
        var access = await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<Cattle>.Failure(access.DomainError);
        }

        var found = await FindAsync(command.RanchId, command.CattleId, cancellationToken);
        if (found.IsFailure)
        {
            return found;
        }

        var earTag = EarTag.Create(command.EarTag);
        if (earTag.IsFailure)
        {
            return Result<Cattle>.Failure(earTag.DomainError);
        }

        var duplicate = await FindDuplicateEarTagAsync(command.RanchId, earTag.Value!, command.CattleId, cancellationToken);
        if (duplicate is not null)
        {
            return Result<Cattle>.Failure(duplicate);
        }

        var update = found.Value!.Update(command, Today());
        if (update.IsFailure)
        {
            return Result<Cattle>.Failure(update.DomainError);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return found;
    }

    /// <inheritdoc />
    public async Task<Result<Cattle>> HandleAsync(PatchCattleCommand command, CancellationToken cancellationToken)
    {
        if (command.CattleId.Value == Guid.Empty)
        {
            return Result<Cattle>.Failure(CattleInformationErrors.CattleIdentifierRequired);
        }

        var animal = await cattles.FindByIdAsNoTrackingAsync(command.CattleId, cancellationToken);
        if (animal is null)
        {
            return Result<Cattle>.Failure(CattleInformationErrors.CattleNotFound);
        }

        var access = await accessService.EnsureManageAccessAsync(animal.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<Cattle>.Failure(access.DomainError);
        }

        var found = await FindAsync(animal.RanchId, command.CattleId, cancellationToken);
        if (found.IsFailure)
        {
            return found;
        }

        if (command.EarTag is { } proposedEarTag)
        {
            var earTag = EarTag.Create(proposedEarTag);
            if (earTag.IsFailure)
            {
                return Result<Cattle>.Failure(earTag.DomainError);
            }

            var duplicate = await FindDuplicateEarTagAsync(animal.RanchId, earTag.Value!, command.CattleId, cancellationToken);
            if (duplicate is not null)
            {
                return Result<Cattle>.Failure(duplicate);
            }
        }

        var patch = found.Value!.Patch(command, Today());
        if (patch.IsFailure)
        {
            return Result<Cattle>.Failure(patch.DomainError);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return found;
    }

    /// <inheritdoc />
    public async Task<Result<Cattle>> HandleAsync(ChangeLifeStageCommand command, CancellationToken cancellationToken)
    {
        var access = await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<Cattle>.Failure(access.DomainError);
        }

        var found = await FindAsync(command.RanchId, command.CattleId, cancellationToken);
        if (found.IsFailure)
        {
            return found;
        }

        var change = found.Value!.ChangeStage(command, Today());
        if (change.IsFailure)
        {
            return Result<Cattle>.Failure(change.DomainError);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return found;
    }

    /// <inheritdoc />
    public async Task<Result<Cattle>> HandleAsync(DeactivateCattleCommand command, CancellationToken cancellationToken)
    {
        var access = await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<Cattle>.Failure(access.DomainError);
        }

        var found = await FindAsync(command.RanchId, command.CattleId, cancellationToken);
        if (found.IsFailure)
        {
            return found;
        }

        var deactivation = found.Value!.Deactivate(command, Today(), timeProvider.GetUtcNow());
        if (deactivation.IsFailure)
        {
            return Result<Cattle>.Failure(deactivation.DomainError);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return found;
    }

    private async Task<Result<Cattle>> FindAsync(Guid ranchId, CattleId cattleId, CancellationToken cancellationToken)
    {
        if (ranchId == Guid.Empty)
        {
            return Result<Cattle>.Failure(CattleInformationErrors.RanchIdentifierRequired);
        }

        if (cattleId.Value == Guid.Empty)
        {
            return Result<Cattle>.Failure(CattleInformationErrors.CattleIdentifierRequired);
        }

        var cattle = await cattles.FindByIdAsync(ranchId, cattleId, cancellationToken);
        return cattle is null
            ? Result<Cattle>.Failure(CattleInformationErrors.CattleNotFound)
            : Result<Cattle>.Success(cattle);
    }

    private async Task<Error?> FindDuplicateEarTagAsync(Guid ranchId, EarTag earTag, CattleId cattleId,
        CancellationToken cancellationToken)
    {
        var holder = await cattles.FindIdByEarTagAsync(ranchId, earTag, cancellationToken);
        return holder is { } conflicting && conflicting != cattleId ? new DuplicateEarTagError(conflicting) : null;
    }

    private DateOnly Today() => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
}
