using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

public sealed record RegisterWaterTroughResource([Required, StringLength(80)] string Name, Guid? LotId);
