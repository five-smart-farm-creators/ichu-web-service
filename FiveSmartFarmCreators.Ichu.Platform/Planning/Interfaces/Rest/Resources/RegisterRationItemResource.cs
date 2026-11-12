using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

public sealed record RegisterRationItemResource(string? Feed, RationKind? Kind, decimal? KilogramsPerHeadPerDay);
