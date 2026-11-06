using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.CommandServices;

public sealed class CattleCommandService(
    ICattleRepository cattleRepository,
    ICattleUnitOfWork unitOfWork,
    ICattleAccessService accessService,
    TimeProvider timeProvider) : ICattleCommandService
{
    public async Task<Cattle> HandleAsync(RegisterCattleCommand command, CancellationToken cancellationToken)
    {
        await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        var cattle = new Cattle(command, Today());
        await EnsureEarTagAvailableAsync(command.RanchId, cattle.EarTag.Value, null, cancellationToken);
        await cattleRepository.AddAsync(cattle, cancellationToken);
        await unitOfWork.CommitAsync(cattle, cancellationToken);
        return cattle;
    }

    public async Task<Cattle> HandleAsync(UpdateCattleCommand command, CancellationToken cancellationToken)
    {
        await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        var cattle = await FindAsync(command.RanchId, command.CattleId, cancellationToken);
        var earTag = new EarTag(command.EarTag);
        await EnsureEarTagAvailableAsync(command.RanchId, earTag.Value, cattle.Id, cancellationToken);
        cattle.Update(command, Today());
        await unitOfWork.CommitAsync(cattle, cancellationToken);
        return cattle;
    }

    public async Task<Cattle> HandleAsync(ChangeLifeStageCommand command, CancellationToken cancellationToken)
    {
        await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        var cattle = await FindAsync(command.RanchId, command.CattleId, cancellationToken);
        cattle.ChangeStage(command, Today());
        await unitOfWork.CommitAsync(cattle, cancellationToken);
        return cattle;
    }

    public async Task<Cattle> HandleAsync(DeactivateCattleCommand command, CancellationToken cancellationToken)
    {
        await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        var cattle = await FindAsync(command.RanchId, command.CattleId, cancellationToken);
        cattle.Deactivate(command, Today());
        await unitOfWork.CommitAsync(cattle, cancellationToken);
        return cattle;
    }

    private async Task<Cattle> FindAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken)
    {
        if (ranchId == Guid.Empty || cattleId == Guid.Empty)
        {
            throw new CattleInformationException("invalid_input", "Ranch and cattle identifiers are required.");
        }

        return await cattleRepository.FindByIdAsync(ranchId, cattleId, cancellationToken)
            ?? throw new CattleInformationException("not_found", "The animal was not found in this ranch.");
    }

    private async Task EnsureEarTagAvailableAsync(Guid ranchId, string earTag, Guid? cattleId,
        CancellationToken cancellationToken)
    {
        var existing = await cattleRepository.FindByEarTagAsync(ranchId, earTag, cancellationToken);
        if (existing is not null && existing.Id != cattleId)
        {
            throw new CattleInformationException("duplicate_ear_tag",
                "Another animal in this ranch already uses this ear tag.", existing.Id);
        }
    }

    private DateOnly Today() => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
}
