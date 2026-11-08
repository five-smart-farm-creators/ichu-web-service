using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.OperationsMonitoring;

/// <summary>
///     Builds valid telemetry, so that each test states only the value it depends on.
/// </summary>
internal static class TelemetryFixtures
{
    public static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    public static readonly Guid GatewayId = Guid.Parse("44444444-4444-4444-8444-444444444444");
    public static readonly Guid DeviceId = Guid.Parse("55555555-5555-4555-8555-555555555555");
    public static readonly Guid CattleId = Guid.Parse("66666666-6666-4666-8666-666666666666");
    public static readonly DateTimeOffset CapturedAt = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);
    public static readonly DateTimeOffset ReceivedAt = CapturedAt.AddMinutes(1);

    public static TelemetryReading Reading(
        double? temperature,
        Guid? readingId = null,
        DateTimeOffset? capturedAt = null,
        PositionMeasurement? position = null,
        bool attributed = true)
    {
        var reading = TelemetryReading.Create(
            readingId ?? Guid.NewGuid(),
            DeviceId,
            capturedAt ?? CapturedAt,
            Measurement.From(temperature, null).Value!,
            Measurement.Absent(),
            Measurement.Absent(),
            Measurement.Absent(),
            position ?? PositionMeasurement.Absent(),
            GatewayId,
            RanchId,
            attributed ? CattleId : null,
            ReceivedAt);
        return reading.Value!;
    }
}
