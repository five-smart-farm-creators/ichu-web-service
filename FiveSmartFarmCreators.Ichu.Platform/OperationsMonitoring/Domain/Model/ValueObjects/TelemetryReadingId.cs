using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Strongly typed identifier of a stored reading, distinct from the firmware reading identifier it is stored under.
/// </summary>
public readonly record struct TelemetryReadingId(Guid Value) : IEntityId<Guid>;
