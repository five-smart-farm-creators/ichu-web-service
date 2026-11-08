using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Strongly typed identifier of a threshold, so that it cannot be mixed up with the ranch that configured it.
/// </summary>
public readonly record struct MonitoringThresholdId(Guid Value) : IEntityId<Guid>;
