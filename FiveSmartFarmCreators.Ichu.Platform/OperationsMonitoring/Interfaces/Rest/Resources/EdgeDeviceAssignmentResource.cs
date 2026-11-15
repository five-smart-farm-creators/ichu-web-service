namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public record EdgeDeviceAssignmentResource(Guid DeviceId, Guid CattleId, Guid? LotId, string LifeStage);
