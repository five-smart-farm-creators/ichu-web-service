using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class WaterTroughStatusResourceFromEntityAssembler
{
    public static WaterTroughStatusResource ToResourceFromEntity(WaterTroughStatus status) => new(
        status.WaterTroughId, status.Name, status.LotId, status.MinimumCelsius, status.MaximumCelsius,
        status.ControllerStatus, status.ControllerId, status.ControllerAssignedAt, status.LastCommunicationAt,
        status.MeasurementStatus, status.RangeStatus,
        status.LatestReading is null ? null : WaterTroughReadingResourceFromEntityAssembler.ToResourceFromEntity(status.LatestReading),
        status.EvaluatedAt);
}
