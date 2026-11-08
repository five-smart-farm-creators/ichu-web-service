namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

public sealed record EdgeGatewayResource(Guid Id, Guid RanchId, string Name, string Status, DateTimeOffset RegisteredAt,
    DateTimeOffset? RevokedAt);
