namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;

public record DeviceAttribution(Guid DeviceId, Guid RanchId, Guid? CattleId);
