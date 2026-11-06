namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Defines the AuthenticatedAccountResource REST representation.</summary>
public sealed record AuthenticatedAccountResource(string IdToken, string TokenType);
