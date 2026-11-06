namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Acl;

/// <summary>
/// Carries the CattleInformation facts required to validate a Planning reference.
/// </summary>
/// <param name="CattleId">The animal identifier.</param>
/// <param name="RanchId">The ranch currently owning the animal.</param>
/// <param name="LotId">The optional lot identifier.</param>
/// <param name="Status">The animal status supplied by CattleInformation.</param>
public sealed record CattleReference(Guid CattleId, Guid RanchId, Guid? LotId, string Status);
