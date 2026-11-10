using System.Collections.Concurrent;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

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

    public Task<ExternalIdentity> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken)
    {
        var parts = idToken.Split('|');
        if (parts is not [TokenPrefix, var uid, var email, var verified] || string.IsNullOrWhiteSpace(uid) ||
            TryCreateEmail(email) is not { } address)
        {
            throw new IamException("invalid_credentials", "The identity token is invalid.");
        }

        return Task.FromResult(new ExternalIdentity(uid, address, verified == "true"));
    }

    public Task<string> SignInAsync(Email email, string password, CancellationToken cancellationToken)
    {
        if (password != CorrectPassword || !_usersByEmail.TryGetValue(email.Value, out var user))
        {
            throw new IamException("invalid_credentials", "Access could not be granted.");
        }

        return Task.FromResult(string.Join('|', TokenPrefix, user.Uid, user.Email.Value, "true"));
    }

    private static Email? TryCreateEmail(string value)
    {
        try
        {
            return new Email(value);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
