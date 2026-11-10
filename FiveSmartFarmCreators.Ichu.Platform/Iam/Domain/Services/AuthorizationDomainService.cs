using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Services;

/// <summary>
///     An owner manages the ranch, and any member reads it. A veterinarian reaches the ranch only through a valid advisory,
///     and never with the manage scope.
/// </summary>
public sealed class AuthorizationDomainService
{
    public bool CanAccessRanch(UserAccount account, Guid ranchId, AccessScope scope,
        IEnumerable<RanchMembership> memberships, IEnumerable<HerdAdvisory> advisories, DateTimeOffset now)
    {
        if (ranchId == Guid.Empty || !Enum.IsDefined(scope) || !account.IsActive(now))
        {
            return false;
        }

        if (memberships.Any(membership => membership.UserAccountId == account.Id && membership.RanchId == ranchId &&
            ((membership.IsOwner && account.Role == UserRole.RanchManager) || scope == AccessScope.Read)))
        {
            return true;
        }

        return account.Role == UserRole.Veterinarian && account.Profile is not null && scope != AccessScope.Manage &&
            advisories.Any(advisory => advisory.VeterinarianId == account.Id && advisory.RanchId == ranchId &&
                advisory.IsValidOn(now) && (scope == AccessScope.Read || advisory.Scope == AccessScope.ClinicalWrite));
    }
}
