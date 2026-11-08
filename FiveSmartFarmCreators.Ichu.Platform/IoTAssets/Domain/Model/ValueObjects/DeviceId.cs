using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

/// <summary>
///     Strongly typed identifier of a device, so that it cannot be mixed up with a water trough or assignment identifier.
/// </summary>
public readonly record struct DeviceId(Guid Value) : IEntityId<Guid>;
