using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

/// <summary>
///     Strongly typed identifier of a device assignment, which keeps history rows apart from the devices they belong to.
/// </summary>
public readonly record struct DeviceAssignmentId(Guid Value) : IEntityId<Guid>;
