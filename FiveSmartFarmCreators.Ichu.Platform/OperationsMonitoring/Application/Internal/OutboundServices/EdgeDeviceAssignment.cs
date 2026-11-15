namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;

/// <summary>
///     A collar and the animal it serves. The animal's lot and life stage select the geofence and thresholds that the Edge Gateway applies to the collar's readings.
/// </summary>
public record EdgeDeviceAssignment(Guid DeviceId, Guid CattleId, Guid? LotId, string LifeStage);
