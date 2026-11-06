namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>
///     Contains the ID token and token type returned after account sign-in.
/// </summary>
public sealed record AuthenticatedAccountResource(string IdToken, string TokenType);
