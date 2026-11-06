namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Queries;

public sealed record GetDeviceByIdQuery(Guid RanchId, Guid DeviceId);
