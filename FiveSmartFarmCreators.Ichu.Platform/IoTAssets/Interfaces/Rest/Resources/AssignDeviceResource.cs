namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

public sealed record AssignDeviceResource(Guid TargetId, Guid ExpectedVersion);
