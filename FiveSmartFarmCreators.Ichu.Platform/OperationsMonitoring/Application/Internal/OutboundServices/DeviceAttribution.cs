namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;

/// <summary>
///     The ranch that owns a device and the animal assigned to it at a given instant, if any.
/// </summary>
/// <param name="DeviceId">The device identifier.</param>
/// <param name="RanchId">The ranch that owns the device.</param>
/// <param name="CattleId">The animal assigned to the device at the requested instant, if any.</param>
public record DeviceAttribution(Guid DeviceId, Guid RanchId, Guid? CattleId);
