namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

/// <summary>
/// Reports device-integration availability separately from an unassigned collar and uses UTC reception timestamps.
/// </summary>
public sealed record CattleDeviceResource(bool IsAvailable, Guid? CollarId, DateTimeOffset? LastReadingReceivedAt);
