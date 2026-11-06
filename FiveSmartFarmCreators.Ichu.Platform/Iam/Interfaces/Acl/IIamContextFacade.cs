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

    /// <summary>
    ///     Resolves the active verified account of the current request.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The account identifier, or null when the request has no active verified account.</returns>
    Task<Guid?> FetchCurrentAccountIdAsync(CancellationToken cancellationToken);
}
