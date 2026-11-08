using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

/// <summary>
///     Strongly typed identifier of an animal, so that it cannot be mixed up with a lot identifier.
/// </summary>
public readonly record struct CattleId(Guid Value) : IEntityId<Guid>;
