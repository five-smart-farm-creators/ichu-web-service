using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

/// <summary>
///     Strongly typed identifier of a herd advisory, so that it cannot be mixed up with an account identifier.
/// </summary>
public readonly record struct HerdAdvisoryId(Guid Value) : IEntityId<Guid>;
