namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

/// <summary>
/// Combines retained animal details with explicit device-integration availability for the profile endpoint.
/// </summary>
public sealed record CattleProfileResource(CattleResource Cattle, CattleDeviceResource Device);
