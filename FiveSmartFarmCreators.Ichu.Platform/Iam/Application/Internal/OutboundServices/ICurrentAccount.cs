using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;

/// <summary>
///     Resolves the authenticated identity associated with the current request.
/// </summary>
public interface ICurrentAccount
{
    /// <summary>
    ///     Returns the current verified account and enforces local access restrictions.
    /// </summary>
    /// <param name="cancellationToken">
    ///     Cancels account resolution for the current request.
    /// </param>
    /// <returns>
    ///     The account associated with the authenticated request.
    /// </returns>
    Task<UserAccount> GetAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Returns the verified provider identity established by request authentication.
    /// </summary>
    ExternalIdentity GetExternalIdentity();
}
