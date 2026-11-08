using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;

public readonly record struct FollowUpId(Guid Value) : IEntityId<Guid>;
