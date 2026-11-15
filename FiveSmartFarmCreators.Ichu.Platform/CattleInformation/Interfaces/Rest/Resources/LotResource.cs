using System.ComponentModel;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

/// <summary>
/// Exposes stored memberships and the lot version; inactive animal memberships remain visible.
/// </summary>
public sealed record LotResource(
    Guid Id,
    Guid RanchId,
    string Name,
    [property: Description("Optional plain-text reason for grouping the animals, displayed in lot lists and exports. Example: Animals in lactation.")]
    string? Criterion,
    Guid Version,
    IReadOnlyList<LotMembershipResource> Memberships);
