using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Queries;

public sealed record GetDeviceAssignmentsQuery(Guid RanchId, DeviceId DeviceId);
