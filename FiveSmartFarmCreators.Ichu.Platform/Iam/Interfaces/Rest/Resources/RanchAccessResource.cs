namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

public sealed record RanchAccessResource(Guid RanchId, string Scope, DateTimeOffset? ExpiresAt);
