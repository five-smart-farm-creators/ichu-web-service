namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>
///     Contains the effective access scope and expiration for a ranch.
/// </summary>
public sealed record RanchAccessResource(Guid RanchId, string Scope, DateTimeOffset? ExpiresAt);
