using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;

/// <summary>
/// The values of one ration item as the caller sent them. Missing values stay null, so that the item reports exactly what is wrong.
/// </summary>
public sealed record RationItemDraft(string? Feed, RationKind? Kind, decimal? KilogramsPerHeadPerDay);
