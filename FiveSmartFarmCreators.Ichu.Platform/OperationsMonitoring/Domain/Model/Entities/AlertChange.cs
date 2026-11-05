using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Entities;

/// <summary>
/// An operation applied to an alert. The operation identifier makes retries idempotent and the fingerprint
/// distinguishes an identical retry from a different operation that reuses the same identifier.
/// </summary>
public sealed class AlertChange
{
    public Guid OperationId { get; private set; }
    public AlertChangeKind Kind { get; private set; }
    public string Fingerprint { get; private set; } = string.Empty;
    public DateTimeOffset AppliedAt { get; private set; }

    private AlertChange()
    {
    }

    public AlertChange(Guid operationId, AlertChangeKind kind, string fingerprint, DateTimeOffset appliedAt)
    {
        OperationId = operationId;
        Kind = kind;
        Fingerprint = fingerprint;
        AppliedAt = appliedAt.ToUniversalTime();
    }
}
