using System.Security.Cryptography;
using System.Text;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

/// <summary>
/// An alert evaluated by an Edge Gateway and synchronized with a stable identifier. The alert is raised at the
/// capture time of its triggering reading, and every supporting reading must satisfy the alert condition.
/// </summary>
public sealed class MonitoringAlert
{
    // US-16 and TS-05: new evidence of the same condition within six hours joins the open alert.
    public static readonly TimeSpan DeduplicationWindow = TimeSpan.FromHours(6);
    public const int MaximumTextLength = 400;

    private readonly List<AlertEvidence> _evidence = [];
    private readonly List<AlertChange> _changes = [];

    public Guid Id { get; private set; }
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
    public Guid? GeofenceId { get; private set; }
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

    private MonitoringAlert()
    {
    }

    private MonitoringAlert(
        Guid alertId,
        AlertType type,
        AlertSeverity severity,
        long configurationRevision,
        Guid gatewayId,
        IReadOnlyList<TelemetryReading> supportingReadings,
        DateTimeOffset receivedAt)
    {
        if (alertId == Guid.Empty || gatewayId == Guid.Empty)
        {
            throw MonitoringException.InvalidInput("Alert and gateway identifiers are required.");
        }

        if (configurationRevision < 1)
        {
            throw MonitoringException.InvalidInput("An alert must reference the configuration revision used to evaluate it.");
        }

        if (supportingReadings.Count == 0)
        {
            throw MonitoringException.InvalidInput("An alert requires at least one supporting reading.");
        }

        var trigger = supportingReadings[0];
        if (trigger.CattleId is not { } cattleId)
        {
            throw MonitoringException.InvalidInput("Alert evidence must be attributed to an animal.");
        }

        Id = alertId;
        RanchId = trigger.RanchId;
        CattleId = cattleId;
        DeviceId = trigger.DeviceId;
        TriggerReadingId = trigger.ReadingId;
        GatewayId = gatewayId;
        Type = type;
        Severity = severity;
        Status = AlertStatus.Pending;
        ConfigurationRevision = configurationRevision;
        RaisedAt = trigger.RecordedAt;
        ReceivedAt = receivedAt.ToUniversalTime();
        Version = 1;
    }

    public ThresholdRange? Threshold => ThresholdMinValue is { } min && ThresholdMaxValue is { } max
        ? new ThresholdRange(min, max)
        : null;

    public TimeSpan? ResponseTime => AcknowledgedAt - RaisedAt;

    public TimeSpan? ExitDuration => ClearedAt - RaisedAt;

    public static MonitoringAlert RaiseForThreshold(
        Guid alertId,
        AlertType type,
        AlertSeverity severity,
        ThresholdRange threshold,
        long configurationRevision,
        Guid gatewayId,
        IReadOnlyList<TelemetryReading> supportingReadings,
        DateTimeOffset receivedAt)
    {
        var metric = MetricFor(type);
        var alert = new MonitoringAlert(alertId, type, severity, configurationRevision, gatewayId, supportingReadings, receivedAt)
        {
            ThresholdMinValue = threshold.MinValue,
            ThresholdMaxValue = threshold.MaxValue,
            DetectedValue = supportingReadings[0].MeasurementOf(metric).Value
        };
        alert.AttachEvidence(supportingReadings);
        return alert;
    }

    public static MonitoringAlert RaiseForGeofence(
        Guid alertId,
        AlertSeverity severity,
        Guid geofenceId,
        double distanceToBoundaryMeters,
        long configurationRevision,
        Guid gatewayId,
        IReadOnlyList<TelemetryReading> supportingReadings,
        DateTimeOffset receivedAt)
    {
        if (geofenceId == Guid.Empty)
        {
            throw MonitoringException.InvalidInput("A geofence alert must reference its geofence.");
        }

        if (!double.IsFinite(distanceToBoundaryMeters) || distanceToBoundaryMeters < 0)
        {
            throw MonitoringException.InvalidInput("The distance to the geofence boundary must be a non-negative number of meters.");
        }

        var alert = new MonitoringAlert(alertId, AlertType.Geofence, severity, configurationRevision, gatewayId, supportingReadings, receivedAt)
        {
            GeofenceId = geofenceId,
            DistanceToBoundaryMeters = distanceToBoundaryMeters,
            Latitude = supportingReadings[0].Position.Latitude,
            Longitude = supportingReadings[0].Position.Longitude
        };
        alert.AttachEvidence(supportingReadings);
        return alert;
    }

    public static MonitoredMetric MetricFor(AlertType type) => type switch
    {
        AlertType.Temperature => MonitoredMetric.BodyTemperature,
        AlertType.Activity => MonitoredMetric.ActivityIndex,
        _ => throw MonitoringException.InvalidInput($"{type} alerts are not evaluated against a metric threshold.")
    };

    public bool IsOpenDuplicateCandidate(AlertType type, Guid cattleId, DateTimeOffset raisedAt) =>
        Status == AlertStatus.Pending &&
        ClearedAt is null &&
        Type == type &&
        CattleId == cattleId &&
        (raisedAt - RaisedAt).Duration() <= DeduplicationWindow;

    // Compares the facts of the original evaluation; evidence absorbed later does not change them.
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

    public AlertChangeOutcome AddEvidence(Guid operationId, IReadOnlyList<TelemetryReading> readings, DateTimeOffset appliedAt)
    {
        if (readings.Count == 0)
        {
            throw MonitoringException.InvalidInput("Evidence requires at least one reading.");
        }

        var fingerprint = Fingerprint(readings.Select(r => $"{r.DeviceId:N}:{r.ReadingId:N}").Order());
        if (FindReplay(operationId, AlertChangeKind.Evidence, fingerprint) is { } replay)
        {
            return replay;
        }

        AttachEvidence(readings);
        _changes.Add(new AlertChange(operationId, AlertChangeKind.Evidence, fingerprint, appliedAt));
        return AlertChangeOutcome.Applied;
    }

    public AlertChangeOutcome Clear(Guid operationId, DateTimeOffset clearedAt, DateTimeOffset appliedAt)
    {
        if (Type != AlertType.Geofence)
        {
            throw MonitoringException.InvalidInput("Only geofence alerts record a return to the permitted area.");
        }

        var at = clearedAt.ToUniversalTime();
        var fingerprint = Fingerprint([at.ToString("O")]);
        if (FindReplay(operationId, AlertChangeKind.Cleared, fingerprint) is { } replay)
        {
            return replay;
        }

        if (at < RaisedAt)
        {
            throw MonitoringException.InvalidInput("The return to the permitted area cannot precede the alert event.");
        }

        if (ClearedAt is not null)
        {
            return AlertChangeOutcome.Conflict;
        }

        ClearedAt = at;
        _changes.Add(new AlertChange(operationId, AlertChangeKind.Cleared, fingerprint, appliedAt));
        return AlertChangeOutcome.Applied;
    }

    public AlertChangeOutcome Acknowledge(
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
            throw MonitoringException.InvalidInput("Alert attention requires its author.");
        }

        var normalizedFinding = NormalizeText(finding, "finding")
            ?? throw MonitoringException.InvalidInput("Alert attention requires a finding.");
        var normalizedAction = NormalizeText(action, "action");

        // The attention time is excluded so that a retried cloud request keeps the original result.
        var fingerprint = Fingerprint([authorId.ToString("N"), normalizedFinding, normalizedAction ?? string.Empty, expectedVersion.ToString()]);
        if (FindReplay(operationId, AlertChangeKind.Acknowledged, fingerprint) is { } replay)
        {
            return replay;
        }

        var at = acknowledgedAt.ToUniversalTime();
        if (at < RaisedAt)
        {
            throw MonitoringException.InvalidInput("Alert attention cannot precede the alert event.");
        }

        if (Status != AlertStatus.Pending || expectedVersion != Version)
        {
            return AlertChangeOutcome.Conflict;
        }

        Status = AlertStatus.Acknowledged;
        AcknowledgedAt = at;
        AcknowledgedBy = authorId;
        Finding = normalizedFinding;
        Action = normalizedAction;
        Version++;
        _changes.Add(new AlertChange(operationId, AlertChangeKind.Acknowledged, fingerprint, appliedAt));
        return AlertChangeOutcome.Applied;
    }

    private void AttachEvidence(IEnumerable<TelemetryReading> readings)
    {
        foreach (var reading in readings)
        {
            if (!Supports(reading))
            {
                throw MonitoringException.InvalidInput($"Reading {reading.ReadingId} does not support the {Type} alert condition.");
            }

            if (!_evidence.Any(e => e.DeviceId == reading.DeviceId && e.ReadingId == reading.ReadingId))
            {
                _evidence.Add(new AlertEvidence(reading.DeviceId, reading.ReadingId));
            }
        }
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

    private AlertChangeOutcome? FindReplay(Guid operationId, AlertChangeKind kind, string fingerprint)
    {
        if (operationId == Guid.Empty)
        {
            throw MonitoringException.InvalidInput("An operation identifier is required.");
        }

        var previous = _changes.FirstOrDefault(c => c.OperationId == operationId);
        if (previous is null)
        {
            return null;
        }

        return previous.Kind == kind && previous.Fingerprint == fingerprint
            ? AlertChangeOutcome.AlreadyApplied
            : AlertChangeOutcome.Conflict;
    }

    private static string? NormalizeText(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > MaximumTextLength)
        {
            throw MonitoringException.InvalidInput($"The {field} cannot exceed {MaximumTextLength} characters.");
        }

        return normalized;
    }

    private static string Fingerprint(IEnumerable<string> parts) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('|', parts))));
}
