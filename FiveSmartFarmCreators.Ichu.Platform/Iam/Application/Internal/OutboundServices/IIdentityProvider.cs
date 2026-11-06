using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;

/// <summary>Delegates credential and identity-token verification to the external provider.</summary>
public interface IIdentityProvider
{
    /// <summary>Verifies an external ID token before accepting its identity claims.</summary>
    Task<ExternalIdentity> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken);
    /// <summary>Authenticates credentials without persisting or issuing replacement credentials.</summary>
    Task<string> SignInAsync(Email email, string password, CancellationToken cancellationToken);
}
