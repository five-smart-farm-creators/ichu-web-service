namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Acl;

public sealed record WaterTroughMonitoringSnapshot(Guid WaterTroughId, Guid RanchId, string Name, Guid? LotId,
    decimal? MinimumCelsius, decimal? MaximumCelsius, Guid? ControllerId, DateTimeOffset? ControllerAssignedAt);
