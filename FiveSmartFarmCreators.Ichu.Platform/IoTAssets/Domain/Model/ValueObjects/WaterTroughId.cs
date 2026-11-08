using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

/// <summary>
///     Strongly typed identifier of a water trough, so that it cannot be mixed up with a device identifier.
/// </summary>
public readonly record struct WaterTroughId(Guid Value) : IEntityId<Guid>;
