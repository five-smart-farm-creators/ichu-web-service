using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

/// <summary>
///     Strongly typed identifier of a lot, so that it cannot be mixed up with an animal identifier.
/// </summary>
public readonly record struct LotId(Guid Value) : IEntityId<Guid>;
