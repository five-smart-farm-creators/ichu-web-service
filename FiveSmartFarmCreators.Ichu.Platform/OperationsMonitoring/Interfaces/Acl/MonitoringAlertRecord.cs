using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;

/// <summary>
///     The lifetime of an alert: raised at the triggering capture time and closed when acknowledged.
/// </summary>
public record MonitoringAlertRecord(
    Guid AlertId,
    Guid CattleId,
    AlertType Type,
    AlertSeverity Severity,
    DateTimeOffset RaisedAt,
    DateTimeOffset? AcknowledgedAt,
    DateTimeOffset? ClearedAt);
