namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;

/// <summary>
///     Describes a fake Firebase user; every instance is unique so that tests never share accounts.
/// </summary>
public sealed record TestIdentity(string Uid, string Email, bool EmailVerified = true)
{
    public static TestIdentity Create(bool emailVerified = true)
    {
        var suffix = Guid.NewGuid().ToString("N");
        return new TestIdentity($"uid-{suffix}", $"manager-{suffix}@example.com", emailVerified);
    }

    public string BearerToken => string.Join('|', "test", Uid, Email, EmailVerified ? "true" : "false");
}
