namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.QueryServices;

/// <summary>
/// Distinguishes unavailable integration data from a confirmed absence of a collar; reception times are UTC.
/// </summary>
public sealed record CattleDeviceDetails(bool IsAvailable, Guid? CollarId, DateTimeOffset? LastReadingReceivedAt);
