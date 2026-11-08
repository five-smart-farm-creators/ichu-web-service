using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Strongly typed identifier of an alert, so that it cannot be mixed up with the operation identifiers clients send.
/// </summary>
public readonly record struct MonitoringAlertId(Guid Value) : IEntityId<Guid>;
