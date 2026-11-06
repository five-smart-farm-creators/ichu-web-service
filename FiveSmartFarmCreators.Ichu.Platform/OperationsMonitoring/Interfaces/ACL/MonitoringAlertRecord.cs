using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.ACL;

/// <summary>
///     The lifetime of an alert: raised at the triggering capture time and closed when acknowledged.
/// </summary>
/// <param name="AlertId">The alert identifier.</param>
/// <param name="CattleId">The animal the alert refers to.</param>
/// <param name="Type">The condition that raised the alert.</param>
/// <param name="Severity">The severity of the alert.</param>
/// <param name="RaisedAt">The capture time of the triggering reading.</param>
/// <param name="AcknowledgedAt">The instant the alert received attention, if it did.</param>
/// <param name="ClearedAt">The instant the animal returned to the permitted area, if it did.</param>
public sealed record MonitoringAlertRecord(
    Guid AlertId,
    Guid CattleId,
    AlertType Type,
    AlertSeverity Severity,
    DateTimeOffset RaisedAt,
    DateTimeOffset? AcknowledgedAt,
    DateTimeOffset? ClearedAt);
