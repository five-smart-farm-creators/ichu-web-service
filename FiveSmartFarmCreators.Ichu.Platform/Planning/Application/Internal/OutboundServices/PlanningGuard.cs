using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;

/// <summary>
/// Applies the checks that depend on other contexts, so every Planning use case refuses the same inputs with the same error.
/// </summary>
public sealed class PlanningGuard(
    IPlanningAccessControl accessControl,
    IPlanningCattleLookup cattleLookup,
    IClinicalInterventionLookup interventionLookup,
    IPlanningLotLookup lotLookup)
{
    public async Task<Result> EnsureRanchAccessAsync(Guid ranchId, PlanningPermission permission, CancellationToken cancellationToken)
    {
        if (ranchId == Guid.Empty)
        {
            return Result.Failure(PlanningErrors.InvalidIdentifier);
        }

        var accountId = await accessControl.GetCurrentAccountIdAsync(cancellationToken);
        if (accountId is not { } verifiedAccountId
            || !await accessControl.CanAccessRanchAsync(verifiedAccountId, ranchId, permission, cancellationToken))
        {
            return Result.Failure(PlanningErrors.RanchAccessDenied);
        }

        return Result.Success();
    }

    public async Task<Result> EnsureCattleBelongsToRanchAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken)
    {
        // An empty identifier is a malformed request, so it is reported before the animal lookup.
        if (cattleId == Guid.Empty)
        {
            return Result.Failure(PlanningErrors.InvalidIdentifier);
        }

        return await cattleLookup.CattleBelongsToRanchAsync(ranchId, cattleId, cancellationToken)
            ? Result.Success()
            : Result.Failure(PlanningErrors.CattleNotFound);
    }

    public async Task<Result<Guid>> EnsureLotAccessAsync(
        Guid lotId,
        PlanningPermission permission,
        CancellationToken cancellationToken)
    {
        if (lotId == Guid.Empty)
        {
            return Result<Guid>.Failure(PlanningErrors.InvalidIdentifier);
        }

        var ranchId = await lotLookup.FindRanchIdAsync(lotId, cancellationToken);
        if (ranchId is not { } resolvedRanchId)
        {
            return Result<Guid>.Failure(PlanningErrors.LotNotFound);
        }

        var access = await EnsureRanchAccessAsync(resolvedRanchId, permission, cancellationToken);
        if (access.IsSuccess)
        {
            return Result<Guid>.Success(resolvedRanchId);
        }

        if (permission != PlanningPermission.ReadPlanning)
        {
            var readAccess = await EnsureRanchAccessAsync(
                resolvedRanchId, PlanningPermission.ReadPlanning, cancellationToken);
            if (readAccess.IsSuccess)
            {
                return Result<Guid>.Failure(access.DomainError);
            }
        }

        return Result<Guid>.Failure(PlanningErrors.LotNotFound);
    }

    public async Task<Result> EnsureInterventionMatchesAsync(
        Guid? interventionId,
        Guid ranchId,
        Guid cattleId,
        CancellationToken cancellationToken)
    {
        if (interventionId is not { } clinicalInterventionId)
        {
            return Result.Success();
        }

        var intervention = await interventionLookup.FindAsync(clinicalInterventionId, cancellationToken);
        if (intervention is null)
        {
            return Result.Failure(PlanningErrors.InterventionNotFound);
        }

        return intervention.RanchId == ranchId && intervention.CattleId == cattleId
            ? Result.Success()
            : Result.Failure(PlanningErrors.InterventionCattleMismatch);
    }
}
