namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

/// <summary>
/// Identifies a stored member and its UTC assignment instant without asserting that the animal is still active.
/// </summary>
public sealed record LotMembershipResource(Guid CattleId, DateTimeOffset AssignedAt);
