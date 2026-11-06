namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Tokens.Firebase.Configuration;

/// <summary>Configures the Firebase project used for identity verification.</summary>
public sealed class FirebaseOptions
{
    public string ProjectId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
