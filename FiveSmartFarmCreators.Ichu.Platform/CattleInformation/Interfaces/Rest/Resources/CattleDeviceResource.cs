namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

public sealed record CattleDeviceResource(bool IsAvailable, Guid? CollarId, DateTimeOffset? LastReadingReceivedAt);
