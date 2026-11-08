using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.OperationsMonitoring.Domain;

public class TelemetryReadingTests
{
    [Fact]
    public void MeasurementFromAFiniteValueIsValid()
    {
        var measurement = Measurement.From(36.5, null);

        Assert.True(measurement.IsSuccess);
        Assert.Equal(MeasurementStatus.Valid, measurement.Value!.Status);
        Assert.Equal(36.5, measurement.Value.Value);
    }

    [Fact]
    public void MeasurementWithAValueAndAReasonIsRejected()
    {
        var measurement = Measurement.From(36.5, "sensor saturated");

        Assert.Equal(OperationsMonitoringErrors.MeasurementValidAndInvalid.Code, measurement.DomainError.Code);
    }

    [Fact]
    public void MeasurementWithANonFiniteValueIsRejected()
    {
        var measurement = Measurement.From(double.NaN, null);

        Assert.Equal(OperationsMonitoringErrors.MeasurementNotFinite.Code, measurement.DomainError.Code);
    }

    [Fact]
    public void MeasurementWithOnlyAReasonIsInvalidAndKeepsNoValue()
    {
        var measurement = Measurement.From(null, "  saturated  ");

        Assert.Equal(MeasurementStatus.Invalid, measurement.Value!.Status);
        Assert.Null(measurement.Value.Value);
        Assert.Equal("saturated", measurement.Value.InvalidReason);
    }

    [Fact]
    public void MeasurementWithAnOverlongReasonIsRejected()
    {
        var measurement = Measurement.From(null, new string('x', Measurement.MaximumReasonLength + 1));

        Assert.Equal(OperationsMonitoringErrors.InvalidMeasurementReason.Code, measurement.DomainError.Code);
    }

    [Fact]
    public void ReadingWithANegativeActivityIndexIsRejected()
    {
        var reading = TelemetryReading.Create(Guid.NewGuid(), TelemetryFixtures.DeviceId, TelemetryFixtures.CapturedAt,
            Measurement.Absent(), Measurement.From(-1, null).Value!, Measurement.Absent(), Measurement.Absent(),
            PositionMeasurement.Absent(), TelemetryFixtures.GatewayId, TelemetryFixtures.RanchId, TelemetryFixtures.CattleId,
            TelemetryFixtures.ReceivedAt);

        Assert.Equal(OperationsMonitoringErrors.ActivityIndexNegative.Code, reading.DomainError.Code);
    }

    [Fact]
    public void ReadingWithBatteryAboveFullChargeIsRejected()
    {
        var reading = TelemetryReading.Create(Guid.NewGuid(), TelemetryFixtures.DeviceId, TelemetryFixtures.CapturedAt,
            Measurement.Absent(), Measurement.Absent(), Measurement.Absent(), Measurement.From(101, null).Value!,
            PositionMeasurement.Absent(), TelemetryFixtures.GatewayId, TelemetryFixtures.RanchId, TelemetryFixtures.CattleId,
            TelemetryFixtures.ReceivedAt);

        Assert.Equal(OperationsMonitoringErrors.BatteryLevelOutOfRange.Code, reading.DomainError.Code);
    }

    [Fact]
    public void ReadingWithoutItsDeviceIsRejected()
    {
        var reading = TelemetryReading.Create(Guid.NewGuid(), Guid.Empty, TelemetryFixtures.CapturedAt,
            Measurement.Absent(), Measurement.Absent(), Measurement.Absent(), Measurement.Absent(),
            PositionMeasurement.Absent(), TelemetryFixtures.GatewayId, TelemetryFixtures.RanchId, TelemetryFixtures.CattleId,
            TelemetryFixtures.ReceivedAt);

        Assert.Equal(OperationsMonitoringErrors.ReadingIdentifiersRequired.Code, reading.DomainError.Code);
    }

    [Fact]
    public void ReadingOlderThanTheStalenessLimitIsStale()
    {
        var reading = TelemetryFixtures.Reading(37);

        Assert.False(reading.IsStale(reading.RecordedAt.AddHours(23)));
        Assert.True(reading.IsStale(reading.RecordedAt.AddHours(25)));
    }
}
