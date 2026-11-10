using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;

/// <summary>
///     Resolves the identity that the request authentication established. Account lookups enforce the verification and lockout rules.
/// </summary>
public interface ICurrentAccount
{
    Task<Result<UserAccount>> GetAsync(CancellationToken cancellationToken);

    Result<ExternalIdentity> GetExternalIdentity();
}
