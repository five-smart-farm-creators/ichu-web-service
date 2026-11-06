namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

public sealed record LotResource(
    Guid Id,
    Guid RanchId,
    string Name,
    string? Criterion,
    Guid Version,
    IReadOnlyList<LotMembershipResource> Memberships);
