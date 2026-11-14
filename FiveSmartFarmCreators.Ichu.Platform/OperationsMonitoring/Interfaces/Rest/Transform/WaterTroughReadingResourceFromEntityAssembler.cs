using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class WaterTroughReadingResourceFromEntityAssembler
{
    public static WaterTroughReadingResource ToResourceFromEntity(WaterTroughReading reading) => new(
        reading.Id, reading.ReadingId, reading.ControllerId, reading.WaterTroughId, reading.RanchId,
        reading.GatewayId, reading.Temperature, reading.TemperatureUnit, reading.IsValid,
        reading.HeaterState, reading.CapturedAt, reading.ReceivedAt, reading.Origin);
}
