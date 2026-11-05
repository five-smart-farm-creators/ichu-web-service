namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Ports;

/// <summary>The ranch that owns a device and the animal assigned to it at a given instant, if any.</summary>
public sealed record DeviceAttribution(Guid DeviceId, Guid RanchId, Guid? CattleId);
