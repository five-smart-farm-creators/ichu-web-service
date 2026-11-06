using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

public sealed record ReleaseDeviceResource([property: Required, StringLength(120)] string Reason, Guid ExpectedVersion);
