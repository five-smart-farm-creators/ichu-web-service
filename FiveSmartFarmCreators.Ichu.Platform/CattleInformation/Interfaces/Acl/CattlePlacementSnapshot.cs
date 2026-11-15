namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;

/// <summary>
///     Lets other contexts place an active animal in its lot and life stage without loading the animal aggregate.
/// </summary>
public sealed record CattlePlacementSnapshot(Guid CattleId, Guid? LotId, string LifeStage);
