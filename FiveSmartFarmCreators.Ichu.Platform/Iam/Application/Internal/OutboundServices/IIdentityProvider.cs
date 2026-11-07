using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;

/// <summary>
///     Delegates credential and identity-token verification to the external provider.
/// </summary>
public interface IIdentityProvider
{
    /// <summary>
    ///     Verifies an external ID token before accepting its identity claims.
    /// </summary>
    /// <param name="idToken">
    ///     The identity token issued by the external provider.
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancels the provider token-verification request.
    /// </param>
    /// <returns>
    ///     The identity claims verified by the provider.
    /// </returns>
    Task<ExternalIdentity> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken);

    /// <summary>
    ///     Authenticates credentials without persisting or issuing replacement credentials.
    /// </summary>
    /// <param name="email">
    ///     The account email address.
    /// </param>
    /// <param name="password">
    ///     The password submitted to the provider.
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancels the credential-authentication request.
    /// </param>
    /// <returns>
    ///     The ID token issued by the provider.
    /// </returns>
    Task<string> SignInAsync(Email email, string password, CancellationToken cancellationToken);
}
