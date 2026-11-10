using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

/// <summary>
///     Strongly typed identifier of an application account, so that it cannot be mixed up with a ranch identifier.
/// </summary>
public readonly record struct UserAccountId(Guid Value) : IEntityId<Guid>;
