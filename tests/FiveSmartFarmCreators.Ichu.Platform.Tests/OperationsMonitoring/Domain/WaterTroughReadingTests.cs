using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.OperationsMonitoring.Domain;

public sealed class WaterTroughReadingTests
{
    private static readonly Guid GatewayId = Guid.NewGuid();
    private static readonly Guid RanchId = Guid.NewGuid();
    private static readonly Guid ControllerId = Guid.NewGuid();
    private static readonly Guid WaterTroughId = Guid.NewGuid();
    private static readonly Guid ReadingId = Guid.NewGuid();
    private static readonly DateTimeOffset CapturedAt = new(2026, 11, 14, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_PreservesMeasurementOriginAndBothTimestamps()
    {
        var receivedAt = CapturedAt.AddSeconds(3);

        var reading = WaterTroughReading.Create(Command(receivedAt: receivedAt));

        Assert.True(reading.IsSuccess, reading.Message);
        Assert.NotEqual(Guid.Empty, reading.Value!.Id);
        Assert.Equal(ReadingId, reading.Value.ReadingId);
        Assert.Equal(ControllerId, reading.Value.ControllerId);
        Assert.Equal(WaterTroughId, reading.Value.WaterTroughId);
        Assert.Equal(RanchId, reading.Value.RanchId);
        Assert.Equal(21.5m, reading.Value.Temperature);
        Assert.Equal(WaterTemperatureUnit.Celsius, reading.Value.TemperatureUnit);
        Assert.True(reading.Value.IsValid);
        Assert.Equal(WaterHeaterState.On, reading.Value.HeaterState);
        Assert.Equal(CapturedAt, reading.Value.CapturedAt);
        Assert.Equal(receivedAt, reading.Value.ReceivedAt);
        Assert.Equal(WaterTroughReading.EdgeGatewayOrigin, reading.Value.Origin);
    }

    [Fact]
    public void Create_PreservesCaptureAheadOfReceiptForApplicationClockSkewValidation()
    {
        var reading = WaterTroughReading.Create(Command(receivedAt: CapturedAt.AddSeconds(-1)));

        Assert.True(reading.IsSuccess, reading.Message);
    }

    [Fact]
    public void Create_RejectsCaptureTimeWithPrecisionTheDatabaseCannotPreserve()
    {
        var command = Command(receivedAt: CapturedAt.AddSeconds(1)) with { CapturedAt = CapturedAt.AddTicks(1) };

        var reading = WaterTroughReading.Create(command);

        Assert.True(reading.IsFailure);
    }

    [Fact]
    public void HasSameObservation_IgnoresGatewayAndReceiptTimeButDetectsChangedContent()
    {
        var created = WaterTroughReading.Create(Command(receivedAt: CapturedAt.AddSeconds(1))).Value!;
        var retry = Command(gatewayId: Guid.NewGuid(), receivedAt: CapturedAt.AddSeconds(30));

        Assert.True(created.HasSameObservation(retry));
        Assert.False(created.HasSameObservation(retry with { Temperature = 22m }));
        Assert.False(created.HasSameObservation(retry with { HeaterState = WaterHeaterState.Off }));
        Assert.False(created.HasSameObservation(retry with { CapturedAt = CapturedAt.AddSeconds(1) }));
    }

    private static RegisterWaterControllerReadingCommand Command(Guid? gatewayId = null,
        DateTimeOffset? receivedAt = null) => new(gatewayId ?? GatewayId, RanchId, ControllerId, WaterTroughId,
        ReadingId, 21.5m, WaterTemperatureUnit.Celsius, true, WaterHeaterState.On, CapturedAt,
        receivedAt ?? CapturedAt.AddSeconds(1));
}
