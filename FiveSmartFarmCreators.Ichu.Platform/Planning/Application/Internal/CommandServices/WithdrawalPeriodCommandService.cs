using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.CommandServices;

/// <summary>
/// Records withdrawal periods after checking clinical permission, the animal's ranch and the linked intervention.
/// </summary>
public sealed class WithdrawalPeriodCommandService(
    IWithdrawalPeriodRepository withdrawalPeriods,
    IUnitOfWork unitOfWork,
    PlanningGuard guard) : IWithdrawalPeriodCommandService
{
    /// <inheritdoc />
    public async Task<Result<WithdrawalPeriod>> HandleAsync(RecordWithdrawalPeriodCommand command, CancellationToken cancellationToken)
    {
        var access = await guard.EnsureRanchAccessAsync(command.RanchId, PlanningPermission.RecordClinicalPlanning, cancellationToken);
        if (access.IsFailure)
        {
            return Result<WithdrawalPeriod>.Failure(access.DomainError);
        }

        var cattle = await guard.EnsureCattleBelongsToRanchAsync(command.RanchId, command.CattleId, cancellationToken);
        if (cattle.IsFailure)
        {
            return Result<WithdrawalPeriod>.Failure(cattle.DomainError);
        }

        var intervention = await guard.EnsureInterventionMatchesAsync(
            command.InterventionId, command.RanchId, command.CattleId, cancellationToken);
        if (intervention.IsFailure)
        {
            return Result<WithdrawalPeriod>.Failure(intervention.DomainError);
        }

        var withdrawalPeriod = WithdrawalPeriod.Create(
            command.RanchId, command.CattleId, command.InterventionId, command.StartsAt, command.EndsAt);
        if (withdrawalPeriod.IsFailure)
        {
            return withdrawalPeriod;
        }

        await withdrawalPeriods.AddAsync(withdrawalPeriod.Value!, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return withdrawalPeriod;
    }
}
