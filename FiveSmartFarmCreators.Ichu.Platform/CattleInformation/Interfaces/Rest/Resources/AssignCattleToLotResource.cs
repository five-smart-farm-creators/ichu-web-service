namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

/// <summary>
/// Supplies the current lot version for a membership assignment; this is not the animal's version.
/// </summary>
public sealed record AssignCattleToLotResource(Guid ExpectedVersion);
