using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Entities;

/// <summary>
///     An operation applied to an alert. The operation identifier makes retries idempotent, and the stored operation data tells an identical retry apart from a different operation that reuses the identifier.
/// </summary>
public class AlertChange
{
    private AlertChange()
    {
    }

    public AlertChange(Guid operationId, AlertChangeKind kind, string operationData, DateTimeOffset appliedAt)
    {
        OperationId = operationId;
        Kind = kind;
        OperationData = operationData;
        AppliedAt = MonitoringTime.Normalize(appliedAt);
    }

    public Guid OperationId { get; private set; }

    public AlertChangeKind Kind { get; private set; }

    public string OperationData { get; private set; } = string.Empty;

    public DateTimeOffset AppliedAt { get; private set; }

    public bool IsSameOperationAs(AlertChangeKind kind, string operationData) =>
        Kind == kind && OperationData == operationData;
}
