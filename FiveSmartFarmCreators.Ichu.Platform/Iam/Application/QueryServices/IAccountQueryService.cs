using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;

/// <summary>
///     Provides read access to the authenticated account and its effective ranch grants.
/// </summary>
public interface IAccountQueryService
{
    /// <summary>
    ///     Returns the current account from the verified request identity.
    /// </summary>
    /// <returns>
    ///     The authenticated account and its current profile data.
    /// </returns>
    Task<UserAccount> HandleAsync(GetCurrentAccountQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists ranches currently accessible to the authenticated account.
    /// </summary>
    /// <returns>
    ///     The effective ranch scopes and their optional expiration times.
    /// </returns>
    Task<IReadOnlyList<RanchAccess>> HandleAsync(GetCurrentRanchesQuery query, CancellationToken cancellationToken = default);
}
