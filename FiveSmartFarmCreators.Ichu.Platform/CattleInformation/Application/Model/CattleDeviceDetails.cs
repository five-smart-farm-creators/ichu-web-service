namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Model;

public sealed record CattleDeviceDetails(bool IsAvailable, Guid? CollarId, DateTimeOffset? LastReadingReceivedAt);
