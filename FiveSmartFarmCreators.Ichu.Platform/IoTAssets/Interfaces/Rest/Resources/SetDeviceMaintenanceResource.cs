using System.Text.Json.Serialization;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

public sealed record SetDeviceMaintenanceResource([property: JsonRequired] bool InMaintenance, Guid ExpectedVersion);
