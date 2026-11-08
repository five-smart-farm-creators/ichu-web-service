using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Strongly typed identifier of a geofence, so that it cannot be mixed up with the lot it applies to.
/// </summary>
public readonly record struct GeofenceId(Guid Value) : IEntityId<Guid>;
