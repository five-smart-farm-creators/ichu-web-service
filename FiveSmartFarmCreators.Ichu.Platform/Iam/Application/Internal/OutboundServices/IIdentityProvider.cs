using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;

/// <summary>
///     Outbound port to the external identity provider. Failures use the IAM error codes: invalid credentials or tokens
///     are <c>InvalidCredentials</c>, and a provider that cannot answer is <c>IdentityProviderUnavailable</c>.
/// </summary>
public interface IIdentityProvider
{
    Task<Result<ExternalIdentity>> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken);

    Task<Result<string>> SignInAsync(Email email, string password, CancellationToken cancellationToken);
}
