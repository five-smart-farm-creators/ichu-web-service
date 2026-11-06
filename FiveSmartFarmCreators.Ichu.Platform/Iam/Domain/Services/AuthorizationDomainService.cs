using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Services;

/// <summary>Evaluates membership and advisory rules for ranch access.</summary>
public sealed class AuthorizationDomainService
{
    /// <summary>Evaluates account status, memberships and effective advisory scope.</summary>
    public bool CanAccessRanch(UserAccount account, Guid ranchId, AccessScope scope,
        IEnumerable<RanchMembership> memberships, IEnumerable<HerdAdvisory> advisories, DateTimeOffset now)
    {
        if (ranchId == Guid.Empty || !Enum.IsDefined(scope) || !account.IsActive(now)) return false;
        if (memberships.Any(m => m.UserAccountId == account.Id && m.RanchId == ranchId &&
            (m.IsOwner && account.Role == UserRole.RanchManager || scope == AccessScope.Read))) return true;
        return account.Role == UserRole.Veterinarian && account.Profile is not null && scope != AccessScope.Manage &&
            advisories.Any(a => a.VeterinarianId == account.Id && a.RanchId == ranchId && a.IsValidOn(now) &&
                (scope == AccessScope.Read || a.Scope == AccessScope.ClinicalWrite));
    }
}
