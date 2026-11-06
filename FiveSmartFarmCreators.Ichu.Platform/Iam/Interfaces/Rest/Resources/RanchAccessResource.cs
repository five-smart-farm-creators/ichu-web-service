namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Defines the RanchAccessResource REST representation.</summary>
public sealed record RanchAccessResource(Guid RanchId, string Scope, DateTimeOffset? ExpiresAt);
