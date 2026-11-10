using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.QueryServices;

/// <summary>
///     Reads the advisory history of a ranch. A ranch manager sees every advisory of the owned ranch, and a veterinarian sees only its own.
/// </summary>
public sealed class HerdAdvisoryQueryService(
    IRanchRegistrationRepository ranches,
    IUserAccountRepository accounts,
    IHerdAdvisoryRepository advisories,
    ICurrentAccount currentAccount,
    TimeProvider clock) : IHerdAdvisoryQueryService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<HerdAdvisoryDetails>>> HandleAsync(GetHerdAdvisoriesByRanchQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.RanchId == Guid.Empty)
        {
            return Result<IReadOnlyList<HerdAdvisoryDetails>>.Failure(IamErrors.InvalidIdentifier);
        }

        var actor = await currentAccount.GetAsync(cancellationToken);
        if (actor.IsFailure)
        {
            return Result<IReadOnlyList<HerdAdvisoryDetails>>.Failure(actor.DomainError);
        }

        var viewer = actor.Value!;
        var memberships = await ranches.FindMembershipsByAccountAsync(viewer.Id, cancellationToken);
        var owner = viewer.Role == UserRole.RanchManager &&
            memberships.Any(membership => membership.RanchId == query.RanchId && membership.IsOwner);
        if (!owner && viewer.Role != UserRole.Veterinarian)
        {
            return Result<IReadOnlyList<HerdAdvisoryDetails>>.Failure(IamErrors.RanchAccessDenied);
        }

        var entries = await advisories.FindByRanchAsNoTrackingAsync(query.RanchId,
            owner ? null : (UserAccountId?)viewer.Id, cancellationToken);
        var now = clock.GetUtcNow();
        var result = new List<HerdAdvisoryDetails>();
        foreach (var entry in entries)
        {
            var veterinarian = await accounts.FindByIdAsNoTrackingAsync(entry.VeterinarianId, cancellationToken);
            var expired = entry.Status == AdvisoryStatus.Granted && entry.ExpiresAt <= now;
            result.Add(new HerdAdvisoryDetails(entry.Id.Value, entry.VeterinarianId.Value, entry.RanchId, entry.Reason,
                expired ? AdvisoryStatus.Expired : entry.Status, entry.Scope, entry.RequestedAt, entry.GrantedAt,
                entry.ExpiresAt, expired ? entry.ExpiresAt : entry.EndedAt, veterinarian?.Profile));
        }

        return Result<IReadOnlyList<HerdAdvisoryDetails>>.Success(result);
    }
}
