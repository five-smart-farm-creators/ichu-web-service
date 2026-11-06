namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Tokens.Firebase.Configuration;

/// <summary>
///     Stores the Firebase project identifier and API key used by IAM identity adapters.
/// </summary>
public sealed class FirebaseOptions
{
    public string ProjectId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
