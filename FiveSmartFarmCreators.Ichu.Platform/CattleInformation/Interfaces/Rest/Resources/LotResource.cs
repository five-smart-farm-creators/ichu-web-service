namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

/// <summary>
/// Exposes stored memberships and the lot version; inactive animal memberships remain visible.
/// </summary>
public sealed record LotResource(
    Guid Id,
    Guid RanchId,
    string Name,
    string? Criterion,
    Guid Version,
    IReadOnlyList<LotMembershipResource> Memberships);
