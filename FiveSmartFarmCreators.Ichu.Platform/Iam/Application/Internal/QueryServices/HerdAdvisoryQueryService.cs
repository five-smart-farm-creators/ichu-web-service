using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.QueryServices;

/// <summary>
///     Reads advisory history authorized for the current account.
/// </summary>
public sealed class HerdAdvisoryQueryService(IIamRepository repository, ICurrentAccount currentAccount, TimeProvider clock)
    : IHerdAdvisoryQueryService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<HerdAdvisoryDetails>> HandleAsync(GetHerdAdvisoriesByRanchQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.RanchId == Guid.Empty) throw new ArgumentException("A ranch identifier is required.");
        var actor = await currentAccount.GetAsync(cancellationToken);
        var memberships = await repository.FindMembershipsAsync(actor.Id, cancellationToken);
        var owner = memberships.Any(m => m.RanchId == query.RanchId && m.IsOwner) && actor.Role == UserRole.RanchManager;
        if (!owner && actor.Role != UserRole.Veterinarian) throw new IamException("forbidden", "Access is not permitted.");
        var entries = await repository.FindAdvisoriesAsync(query.RanchId, owner ? null : actor.Id, cancellationToken);
        var result = new List<HerdAdvisoryDetails>();
        var now = clock.GetUtcNow();
        foreach (var entry in entries)
        {
            var veterinarian = await repository.FindAccountAsync(entry.VeterinarianId, cancellationToken);
            var expired = entry.Status == AdvisoryStatus.Granted && entry.ExpiresAt <= now;
            result.Add(new HerdAdvisoryDetails(entry.Id, entry.VeterinarianId, entry.RanchId, entry.Reason,
                expired ? AdvisoryStatus.Expired : entry.Status, entry.Scope, entry.RequestedAt, entry.GrantedAt,
                entry.ExpiresAt, expired ? entry.ExpiresAt : entry.EndedAt, veterinarian?.Profile));
        }
        return result;
    }
}
