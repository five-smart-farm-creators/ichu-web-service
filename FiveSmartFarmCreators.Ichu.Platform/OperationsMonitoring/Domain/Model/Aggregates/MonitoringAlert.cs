using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

/// <summary>
///     An alert evaluated by an Edge Gateway and identified by the gateway. It is raised at the capture time of its triggering reading, and every supporting reading must satisfy the alert condition.
/// </summary>
public sealed class MonitoringAlert : AggregateRoot
{
    public const int MaximumTextLength = 400;

    // Window in which new evidence of the same condition joins the open alert instead of raising another.
    public static readonly TimeSpan DeduplicationWindow = TimeSpan.FromHours(6);

    // Separates the fields of the operation data so that different field values never produce the same text.
    private const char FieldSeparator = '\u001F';

    private readonly List<AlertEvidence> _evidence = [];
    private readonly List<AlertChange> _changes = [];

    // Entity Framework Core materializes alerts without running the factories.
    private MonitoringAlert()
    {
    }

    public MonitoringAlertId Id { get; private set; }

    public Guid RanchId { get; private set; }

    public Guid CattleId { get; private set; }

    public Guid DeviceId { get; private set; }

    public Guid TriggerReadingId { get; private set; }

    public Guid GatewayId { get; private set; }

    public AlertType Type { get; private set; }

    public AlertSeverity Severity { get; private set; }

    public AlertStatus Status { get; private set; }

    public double? DetectedValue { get; private set; }

    public double? ThresholdMinValue { get; private set; }

    public double? ThresholdMaxValue { get; private set; }

    public long ConfigurationRevision { get; private set; }

    public GeofenceId? GeofenceId { get; private set; }

    public double? Latitude { get; private set; }

    public double? Longitude { get; private set; }

    public double? DistanceToBoundaryMeters { get; private set; }

    public DateTimeOffset RaisedAt { get; private set; }

    public DateTimeOffset ReceivedAt { get; private set; }

    public DateTimeOffset? ClearedAt { get; private set; }

    public DateTimeOffset? AcknowledgedAt { get; private set; }

    public Guid? AcknowledgedBy { get; private set; }

    public string? Finding { get; private set; }

    public string? Action { get; private set; }

    public int Version { get; private set; }

    public IReadOnlyCollection<AlertEvidence> Evidence => _evidence;

    public IReadOnlyCollection<AlertChange> Changes => _changes;

    public ThresholdRange? Threshold => ThresholdMinValue is { } min && ThresholdMaxValue is { } max
        ? new ThresholdRange(min, max)
        : null;

    public TimeSpan? ResponseTime => AcknowledgedAt - RaisedAt;

    public TimeSpan? ExitDuration => ClearedAt - RaisedAt;

    public static Result<MonitoringAlert> RaiseForThreshold(
        MonitoringAlertId alertId,
        AlertType type,
        AlertSeverity severity,
        ThresholdRange threshold,
        long configurationRevision,
        Guid gatewayId,
        IReadOnlyList<TelemetryReading> supportingReadings,
        DateTimeOffset receivedAt)
    {
        if (type is not (AlertType.Temperature or AlertType.Activity or AlertType.HeartRate or AlertType.Methane))
        {
            return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.AlertTypeNotThreshold);
        }

        var creation = Create(alertId, type, severity, configurationRevision, gatewayId, supportingReadings, receivedAt);
        if (creation.IsFailure)
        {
            return creation;
        }

        var alert = creation.Value!;
        alert.ThresholdMinValue = threshold.MinValue;
        alert.ThresholdMaxValue = threshold.MaxValue;
        alert.DetectedValue = supportingReadings[0].MeasurementOf(MetricFor(type)).Value;
        var attachment = alert.AttachEvidence(supportingReadings);
        return attachment.IsFailure ? Result<MonitoringAlert>.Failure(attachment.DomainError) : creation;
    }

    public static Result<MonitoringAlert> RaiseForGeofence(
        MonitoringAlertId alertId,
        AlertSeverity severity,
        GeofenceId geofenceId,
        double distanceToBoundaryMeters,
        long configurationRevision,
        Guid gatewayId,
        IReadOnlyList<TelemetryReading> supportingReadings,
        DateTimeOffset receivedAt)
    {
        if (geofenceId.Value == Guid.Empty)
        {
            return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.GeofenceAlertRequiresGeofence);
        }

        if (!double.IsFinite(distanceToBoundaryMeters) || distanceToBoundaryMeters < 0)
        {
            return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.GeofenceAlertDistanceInvalid);
        }

        var creation = Create(alertId, AlertType.Geofence, severity, configurationRevision, gatewayId, supportingReadings, receivedAt);
        if (creation.IsFailure)
        {
            return creation;
        }

        var alert = creation.Value!;
        alert.GeofenceId = geofenceId;
        alert.DistanceToBoundaryMeters = distanceToBoundaryMeters;
        alert.Latitude = supportingReadings[0].Position.Latitude;
        alert.Longitude = supportingReadings[0].Position.Longitude;
        var attachment = alert.AttachEvidence(supportingReadings);
        return attachment.IsFailure ? Result<MonitoringAlert>.Failure(attachment.DomainError) : creation;
    }

    // A more severe alert is an escalation, so it is never absorbed by a milder open one.
    public bool IsOpenDuplicateCandidate(AlertType type, AlertSeverity severity, Guid cattleId, DateTimeOffset raisedAt) =>
        Status == AlertStatus.Pending &&
        ClearedAt is null &&
        Type == type &&
        Severity >= severity &&
        CattleId == cattleId &&
        (raisedAt - RaisedAt).Duration() <= DeduplicationWindow;

    // Evidence absorbed later does not change the original evaluation, so it is excluded from the comparison.
    public bool HasSameOriginAs(MonitoringAlert other) =>
        Id == other.Id &&
        Type == other.Type &&
        Severity == other.Severity &&
        CattleId == other.CattleId &&
        DeviceId == other.DeviceId &&
        TriggerReadingId == other.TriggerReadingId &&
        ThresholdMinValue == other.ThresholdMinValue &&
        ThresholdMaxValue == other.ThresholdMaxValue &&
        ConfigurationRevision == other.ConfigurationRevision &&
        GeofenceId == other.GeofenceId &&
        DistanceToBoundaryMeters == other.DistanceToBoundaryMeters;

    public bool HasApplied(Guid operationId) => _changes.Any(c => c.OperationId == operationId);

    public Result<AlertChangeOutcome> AddEvidence(Guid operationId, IReadOnlyList<TelemetryReading> readings, DateTimeOffset appliedAt)
    {
        if (readings.Count == 0)
        {
            return Result<AlertChangeOutcome>.Failure(OperationsMonitoringErrors.EvidenceRequiresReading);
        }

        if (operationId == Guid.Empty)
        {
            return Result<AlertChangeOutcome>.Failure(OperationsMonitoringErrors.OperationIdentifierRequired);
        }

        var operationData = string.Join(',', readings.Select(r => $"{r.DeviceId:N}:{r.ReadingId:N}").Order());
        if (FindReplay(operationId, AlertChangeKind.Evidence, operationData) is { } replay)
        {
            return Result<AlertChangeOutcome>.Success(replay);
        }

        var attachment = AttachEvidence(readings);
        if (attachment.IsFailure)
        {
            return Result<AlertChangeOutcome>.Failure(attachment.DomainError);
        }

        _changes.Add(new AlertChange(operationId, AlertChangeKind.Evidence, operationData, appliedAt));
        return Result<AlertChangeOutcome>.Success(AlertChangeOutcome.Applied);
    }

    public Result<AlertChangeOutcome> Clear(Guid operationId, DateTimeOffset clearedAt, DateTimeOffset appliedAt)
    {
        if (Type != AlertType.Geofence)
        {
            return Result<AlertChangeOutcome>.Failure(OperationsMonitoringErrors.ReturnRequiresGeofenceAlert);
        }

        var at = MonitoringTime.Normalize(clearedAt);
        var operationData = at.ToString("O");
        if (operationId == Guid.Empty)
        {
            return Result<AlertChangeOutcome>.Failure(OperationsMonitoringErrors.OperationIdentifierRequired);
        }

        if (FindReplay(operationId, AlertChangeKind.Cleared, operationData) is { } replay)
        {
            return Result<AlertChangeOutcome>.Success(replay);
        }

        if (at < RaisedAt)
        {
            return Result<AlertChangeOutcome>.Failure(OperationsMonitoringErrors.ReturnPrecedesAlert);
        }

        if (ClearedAt is not null)
        {
            return Result<AlertChangeOutcome>.Success(AlertChangeOutcome.Conflict);
        }

        ClearedAt = at;
        _changes.Add(new AlertChange(operationId, AlertChangeKind.Cleared, operationData, appliedAt));
        return Result<AlertChangeOutcome>.Success(AlertChangeOutcome.Applied);
    }

    public Result<AlertChangeOutcome> Acknowledge(
        Guid operationId,
        Guid authorId,
        string finding,
        string? action,
        DateTimeOffset acknowledgedAt,
        int expectedVersion,
        DateTimeOffset appliedAt)
    {
        if (authorId == Guid.Empty)
        {
            return Result<AlertChangeOutcome>.Failure(OperationsMonitoringErrors.AttentionAuthorRequired);
        }

        var normalizedFinding = TrimmedOrNull(finding);
        if (normalizedFinding is null)
        {
            return Result<AlertChangeOutcome>.Failure(OperationsMonitoringErrors.AttentionFindingRequired);
        }

        if (normalizedFinding.Length > MaximumTextLength)
        {
            return Result<AlertChangeOutcome>.Failure(OperationsMonitoringErrors.AttentionFindingTooLong);
        }

        var normalizedAction = TrimmedOrNull(action);
        if (normalizedAction?.Length > MaximumTextLength)
        {
            return Result<AlertChangeOutcome>.Failure(OperationsMonitoringErrors.AttentionActionTooLong);
        }

        if (operationId == Guid.Empty)
        {
            return Result<AlertChangeOutcome>.Failure(OperationsMonitoringErrors.OperationIdentifierRequired);
        }

        // The attention time is excluded so that a retried cloud request keeps the original result.
        var operationData = string.Join(FieldSeparator, authorId.ToString("N"), normalizedFinding, normalizedAction ?? string.Empty, expectedVersion);
        if (FindReplay(operationId, AlertChangeKind.Acknowledged, operationData) is { } replay)
        {
            return Result<AlertChangeOutcome>.Success(replay);
        }

        var at = MonitoringTime.Normalize(acknowledgedAt);
        if (at < RaisedAt)
        {
            return Result<AlertChangeOutcome>.Failure(OperationsMonitoringErrors.AttentionPrecedesAlert);
        }

        if (Status != AlertStatus.Pending || expectedVersion != Version)
        {
            return Result<AlertChangeOutcome>.Success(AlertChangeOutcome.Conflict);
        }

        Status = AlertStatus.Acknowledged;
        AcknowledgedAt = at;
        AcknowledgedBy = authorId;
        Finding = normalizedFinding;
        Action = normalizedAction;
        Version++;
        _changes.Add(new AlertChange(operationId, AlertChangeKind.Acknowledged, operationData, appliedAt));
        return Result<AlertChangeOutcome>.Success(AlertChangeOutcome.Applied);
    }

    private static Result<MonitoringAlert> Create(
        MonitoringAlertId alertId,
        AlertType type,
        AlertSeverity severity,
        long configurationRevision,
        Guid gatewayId,
        IReadOnlyList<TelemetryReading> supportingReadings,
        DateTimeOffset receivedAt)
    {
        if (alertId.Value == Guid.Empty || gatewayId == Guid.Empty)
        {
            return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.AlertIdentifiersRequired);
        }

        if (configurationRevision < 1)
        {
            return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.AlertRevisionRequired);
        }

        if (supportingReadings.Count == 0)
        {
            return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.AlertSupportingReadingRequired);
        }

        var trigger = supportingReadings[0];
        if (trigger.CattleId is not { } cattleId)
        {
            return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.AlertEvidenceNotAttributed);
        }

        return Result<MonitoringAlert>.Success(new MonitoringAlert
        {
            Id = alertId,
            RanchId = trigger.RanchId,
            CattleId = cattleId,
            DeviceId = trigger.DeviceId,
            TriggerReadingId = trigger.ReadingId,
            GatewayId = gatewayId,
            Type = type,
            Severity = severity,
            Status = AlertStatus.Pending,
            ConfigurationRevision = configurationRevision,
            RaisedAt = trigger.RecordedAt,
            ReceivedAt = MonitoringTime.Normalize(receivedAt),
            Version = 1
        });
    }

    // Threshold alerts are only raised for metrics, so reaching the default branch is a programming error.
    private static MonitoredMetric MetricFor(AlertType type) => type switch
    {
        AlertType.Temperature => MonitoredMetric.BodyTemperature,
        AlertType.Activity => MonitoredMetric.ActivityIndex,
        AlertType.HeartRate => MonitoredMetric.HeartRate,
        AlertType.Methane => MonitoredMetric.MethaneIndex,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    private static string? TrimmedOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    // Every reading is checked before any is attached, so a rejected operation leaves the alert unchanged.
    private Result AttachEvidence(IReadOnlyList<TelemetryReading> readings)
    {
        if (!readings.All(Supports))
        {
            return Result.Failure(OperationsMonitoringErrors.ReadingNotSupportsAlert);
        }

        foreach (var reading in readings)
        {
            if (!_evidence.Any(e => e.DeviceId == reading.DeviceId && e.ReadingId == reading.ReadingId))
            {
                _evidence.Add(new AlertEvidence(reading.DeviceId, reading.ReadingId));
            }
        }

        return Result.Success();
    }

    private bool Supports(TelemetryReading reading)
    {
        if (reading.CattleId != CattleId || reading.RanchId != RanchId)
        {
            return false;
        }

        if (Type == AlertType.Geofence)
        {
            return reading.Position.IsValid;
        }

        return reading.MeasurementOf(MetricFor(Type)).Value is { } value && Threshold!.IsExceededBy(value);
    }

    private AlertChangeOutcome? FindReplay(Guid operationId, AlertChangeKind kind, string operationData)
    {
        var previous = _changes.FirstOrDefault(c => c.OperationId == operationId);
        if (previous is null)
        {
            return null;
        }

        return previous.IsSameOperationAs(kind, operationData)
            ? AlertChangeOutcome.AlreadyApplied
            : AlertChangeOutcome.Conflict;
    }
}
