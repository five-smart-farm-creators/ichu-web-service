using System.Collections.Concurrent;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;

/// <summary>
///     Replaces Firebase so that tests control identities and never reach the network.
/// </summary>
public sealed class FakeIdentityProvider : IIdentityProvider
{
    public const string CorrectPassword = "correct-password";

    private const string TokenPrefix = "test";
    private readonly ConcurrentDictionary<string, ExternalIdentity> _usersByEmail = new();

    /// <summary>
    ///     Makes the identity known to password sign-in, which only succeeds for registered users.
    /// </summary>
    public void Register(string uid, string email)
    {
        var address = new Email(email);
        _usersByEmail[address.Value] = new ExternalIdentity(uid, address, EmailVerified: true);
    }

    public Task<Result<ExternalIdentity>> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken)
    {
        var parts = idToken.Split('|');
        if (parts is not [TokenPrefix, var uid, var email, var verified] || string.IsNullOrWhiteSpace(uid))
        {
            return Task.FromResult(Result<ExternalIdentity>.Failure(IamErrors.InvalidCredentials));
        }

        var address = Email.Create(email);
        return Task.FromResult(address.IsSuccess
            ? Result<ExternalIdentity>.Success(new ExternalIdentity(uid, address.Value!, verified == "true"))
            : Result<ExternalIdentity>.Failure(IamErrors.InvalidCredentials));
    }

    public Task<Result<string>> SignInAsync(Email email, string password, CancellationToken cancellationToken)
    {
        if (password != CorrectPassword || !_usersByEmail.TryGetValue(email.Value, out var user))
        {
            return Task.FromResult(Result<string>.Failure(IamErrors.InvalidCredentials));
        }

        return Task.FromResult(Result<string>.Success(string.Join('|', TokenPrefix, user.Uid, user.Email.Value, "true")));
    }
}
