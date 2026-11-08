using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.OperationsMonitoring.Domain;

public class MonitoringAlertTests
{
    private static readonly MonitoringAlertId AlertId = new(Guid.Parse("22222222-2222-4222-8222-222222222222"));
    private static readonly ThresholdRange FeverRange = ThresholdRange.Create(35, 39).Value!;
    private static readonly DateTimeOffset ReceivedAt = TelemetryFixtures.ReceivedAt;

    [Fact]
    public void ThresholdAlertRecordsTheTriggerAndItsEvidence()
    {
        var trigger = TelemetryFixtures.Reading(40);

        var alert = RaiseFever(trigger).Value!;

        Assert.Equal(AlertStatus.Pending, alert.Status);
        Assert.Equal(1, alert.Version);
        Assert.Equal(40, alert.DetectedValue);
        Assert.Equal(trigger.RecordedAt, alert.RaisedAt);
        Assert.Single(alert.Evidence);
    }

    [Fact]
    public void ThresholdAlertRejectsAReadingWithinTheRange()
    {
        var result = RaiseFever(TelemetryFixtures.Reading(37));

        Assert.Equal(OperationsMonitoringErrors.ReadingNotSupportsAlert.Code, result.DomainError.Code);
    }

    [Fact]
    public void ThresholdAlertRejectsTheGeofenceType()
    {
        var result = MonitoringAlert.RaiseForThreshold(AlertId, AlertType.Geofence, AlertSeverity.Warning, FeverRange, 1,
            TelemetryFixtures.GatewayId, [TelemetryFixtures.Reading(40)], ReceivedAt);

        Assert.Equal(OperationsMonitoringErrors.AlertTypeNotThreshold.Code, result.DomainError.Code);
    }

    [Fact]
    public void AlertWithoutAPublishedRevisionIsRejected()
    {
        var result = MonitoringAlert.RaiseForThreshold(AlertId, AlertType.Temperature, AlertSeverity.Warning, FeverRange, 0,
            TelemetryFixtures.GatewayId, [TelemetryFixtures.Reading(40)], ReceivedAt);

        Assert.Equal(OperationsMonitoringErrors.AlertRevisionRequired.Code, result.DomainError.Code);
    }

    [Fact]
    public void GeofenceAlertRecordsTheDistanceAndTheTriggeringPosition()
    {
        var alert = RaiseGeofence().Value!;

        Assert.Equal(AlertType.Geofence, alert.Type);
        Assert.Equal(12.5, alert.DistanceToBoundaryMeters);
        Assert.Equal(-12.1, alert.Latitude);
        Assert.Equal(-77.0, alert.Longitude);
    }

    [Fact]
    public void GeofenceAlertRejectsANegativeDistance()
    {
        var result = MonitoringAlert.RaiseForGeofence(AlertId, AlertSeverity.Critical, new GeofenceId(Guid.NewGuid()), -1, 1,
            TelemetryFixtures.GatewayId, [GeofenceTrigger()], ReceivedAt);

        Assert.Equal(OperationsMonitoringErrors.GeofenceAlertDistanceInvalid.Code, result.DomainError.Code);
    }

    [Fact]
    public void AcknowledgementAppliesOnceAndAReplayKeepsTheResult()
    {
        var alert = RaiseFever(TelemetryFixtures.Reading(40)).Value!;
        var operationId = Guid.NewGuid();
        var author = Guid.NewGuid();

        var first = alert.Acknowledge(operationId, author, "  Fever confirmed  ", "Isolated", ReceivedAt.AddMinutes(10), 1, ReceivedAt);
        var replay = alert.Acknowledge(operationId, author, "Fever confirmed", "Isolated", ReceivedAt.AddMinutes(30), 1, ReceivedAt);

        Assert.Equal(AlertChangeOutcome.Applied, first.Value);
        Assert.Equal(AlertChangeOutcome.AlreadyApplied, replay.Value);
        Assert.Equal(AlertStatus.Acknowledged, alert.Status);
        Assert.Equal("Fever confirmed", alert.Finding);
        Assert.Equal(2, alert.Version);
    }

    [Fact]
    public void AcknowledgementWithAStaleVersionConflictsAndChangesNothing()
    {
        var alert = RaiseFever(TelemetryFixtures.Reading(40)).Value!;

        var outcome = alert.Acknowledge(Guid.NewGuid(), Guid.NewGuid(), "Checked", null, ReceivedAt, 0, ReceivedAt);

        Assert.Equal(AlertChangeOutcome.Conflict, outcome.Value);
        Assert.Equal(AlertStatus.Pending, alert.Status);
    }

    [Fact]
    public void AnotherOperationAfterAcknowledgementConflicts()
    {
        var alert = RaiseFever(TelemetryFixtures.Reading(40)).Value!;
        alert.Acknowledge(Guid.NewGuid(), Guid.NewGuid(), "Checked", null, ReceivedAt, 1, ReceivedAt);

        var outcome = alert.Acknowledge(Guid.NewGuid(), Guid.NewGuid(), "Checked again", null, ReceivedAt, 2, ReceivedAt);

        Assert.Equal(AlertChangeOutcome.Conflict, outcome.Value);
    }

    [Fact]
    public void AcknowledgementRejectsAFindingLongerThanTheLimit()
    {
        var alert = RaiseFever(TelemetryFixtures.Reading(40)).Value!;

        var outcome = alert.Acknowledge(Guid.NewGuid(), Guid.NewGuid(), new string('x', MonitoringAlert.MaximumTextLength + 1), null,
            ReceivedAt, 1, ReceivedAt);

        Assert.Equal(OperationsMonitoringErrors.AttentionFindingTooLong.Code, outcome.DomainError.Code);
    }

    [Fact]
    public void AcknowledgementRejectsABlankFindingAndAnAttentionBeforeTheAlert()
    {
        var alert = RaiseFever(TelemetryFixtures.Reading(40)).Value!;

        Assert.Equal(OperationsMonitoringErrors.AttentionFindingRequired.Code,
            alert.Acknowledge(Guid.NewGuid(), Guid.NewGuid(), "   ", null, ReceivedAt, 1, ReceivedAt).DomainError.Code);
        Assert.Equal(OperationsMonitoringErrors.AttentionPrecedesAlert.Code,
            alert.Acknowledge(Guid.NewGuid(), Guid.NewGuid(), "Checked", null, alert.RaisedAt.AddMinutes(-1), 1, ReceivedAt).DomainError.Code);
    }

    [Fact]
    public void ReturnToThePermittedAreaIsRecordedOnlyOnceForGeofenceAlerts()
    {
        var alert = RaiseGeofence().Value!;

        var first = alert.Clear(Guid.NewGuid(), ReceivedAt.AddHours(1), ReceivedAt);
        var second = alert.Clear(Guid.NewGuid(), ReceivedAt.AddHours(2), ReceivedAt);

        Assert.Equal(AlertChangeOutcome.Applied, first.Value);
        Assert.Equal(AlertChangeOutcome.Conflict, second.Value);
        Assert.NotNull(alert.ClearedAt);
    }

    [Fact]
    public void ReturnIsRejectedForThresholdAlertsAndBeforeTheAlert()
    {
        var thresholdAlert = RaiseFever(TelemetryFixtures.Reading(40)).Value!;
        var geofenceAlert = RaiseGeofence().Value!;

        Assert.Equal(OperationsMonitoringErrors.ReturnRequiresGeofenceAlert.Code,
            thresholdAlert.Clear(Guid.NewGuid(), ReceivedAt, ReceivedAt).DomainError.Code);
        Assert.Equal(OperationsMonitoringErrors.ReturnPrecedesAlert.Code,
            geofenceAlert.Clear(Guid.NewGuid(), geofenceAlert.RaisedAt.AddMinutes(-1), ReceivedAt).DomainError.Code);
    }

    [Fact]
    public void SupportingReadingJoinsTheAlertOnceAndAReplayDoesNotDuplicateIt()
    {
        var alert = RaiseFever(TelemetryFixtures.Reading(40)).Value!;
        var operationId = Guid.NewGuid();
        var extra = TelemetryFixtures.Reading(40.2, capturedAt: TelemetryFixtures.CapturedAt.AddMinutes(20));

        var first = alert.AddEvidence(operationId, [extra], ReceivedAt);
        var replay = alert.AddEvidence(operationId, [extra], ReceivedAt.AddMinutes(1));

        Assert.Equal(AlertChangeOutcome.Applied, first.Value);
        Assert.Equal(AlertChangeOutcome.AlreadyApplied, replay.Value);
        Assert.Equal(2, alert.Evidence.Count);
    }

    [Fact]
    public void ReadingThatDoesNotSupportTheAlertLeavesItUnchanged()
    {
        var alert = RaiseFever(TelemetryFixtures.Reading(40)).Value!;

        var outcome = alert.AddEvidence(Guid.NewGuid(), [TelemetryFixtures.Reading(37)], ReceivedAt);

        Assert.Equal(OperationsMonitoringErrors.ReadingNotSupportsAlert.Code, outcome.DomainError.Code);
        Assert.Single(alert.Evidence);
    }

    [Fact]
    public void OpenAlertAbsorbsNewEvidenceOnlyWithinTheDeduplicationWindow()
    {
        var alert = RaiseFever(TelemetryFixtures.Reading(40)).Value!;

        Assert.True(alert.IsOpenDuplicateCandidate(AlertType.Temperature, TelemetryFixtures.CattleId, alert.RaisedAt.AddHours(5)));
        Assert.False(alert.IsOpenDuplicateCandidate(AlertType.Temperature, TelemetryFixtures.CattleId, alert.RaisedAt.AddHours(7)));
        Assert.False(alert.IsOpenDuplicateCandidate(AlertType.Activity, TelemetryFixtures.CattleId, alert.RaisedAt));
    }

    [Fact]
    public void AlertsEvaluatedTheSameWayHaveTheSameOrigin()
    {
        var trigger = TelemetryFixtures.Reading(40, readingId: Guid.Parse("77777777-7777-4777-8777-777777777777"));
        var first = RaiseFever(trigger).Value!;
        var second = RaiseFever(trigger).Value!;
        var revised = MonitoringAlert.RaiseForThreshold(AlertId, AlertType.Temperature, AlertSeverity.Warning, FeverRange, 2,
            TelemetryFixtures.GatewayId, [trigger], ReceivedAt).Value!;

        Assert.True(first.HasSameOriginAs(second));
        Assert.False(first.HasSameOriginAs(revised));
    }

    private static Result<MonitoringAlert> RaiseFever(TelemetryReading trigger) =>
        MonitoringAlert.RaiseForThreshold(AlertId, AlertType.Temperature, AlertSeverity.Warning, FeverRange, 1,
            TelemetryFixtures.GatewayId, [trigger], ReceivedAt);

    private static Result<MonitoringAlert> RaiseGeofence() =>
        MonitoringAlert.RaiseForGeofence(AlertId, AlertSeverity.Critical, new GeofenceId(Guid.NewGuid()), 12.5, 1,
            TelemetryFixtures.GatewayId, [GeofenceTrigger()], ReceivedAt);

    private static TelemetryReading GeofenceTrigger() =>
        TelemetryFixtures.Reading(null, position: PositionMeasurement.From(-12.1, -77.0, 5, null).Value!);
}
