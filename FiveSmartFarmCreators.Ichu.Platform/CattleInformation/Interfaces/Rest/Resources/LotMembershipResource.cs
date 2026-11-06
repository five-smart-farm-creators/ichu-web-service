namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

public sealed record LotMembershipResource(Guid CattleId, DateTimeOffset AssignedAt);
