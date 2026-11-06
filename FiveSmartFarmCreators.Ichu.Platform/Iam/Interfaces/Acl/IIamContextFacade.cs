using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

/// <summary>
///     Exposes server-side IAM authorization decisions to other contexts.
/// </summary>
public interface IIamContextFacade
{
    /// <summary>
    ///     Lists effective ranch access for a trusted server-side account identifier.
    /// </summary>
    Task<IReadOnlyList<RanchAccess>> GetAccessibleRanchesAsync(Guid accountId, CancellationToken cancellationToken);
    /// <summary>
    ///     Checks a requested scope, including grant expiry and account lockout.
    /// </summary>
    Task<bool> CanAccessRanchAsync(Guid accountId, Guid ranchId, AccessScope scope, CancellationToken cancellationToken);
}
