using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

public interface IIamContextFacade
{
    Task<IReadOnlyList<RanchAccess>> GetAccessibleRanchesAsync(Guid accountId, CancellationToken cancellationToken);

    /// <summary>
    ///     Checks a requested scope, including grant expiry and account lockout.
    /// </summary>
    Task<bool> CanAccessRanchAsync(Guid accountId, Guid ranchId, AccessScope scope, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns null when the request has no active verified account.
    /// </summary>
    Task<Guid?> FetchCurrentAccountIdAsync(CancellationToken cancellationToken);
}
