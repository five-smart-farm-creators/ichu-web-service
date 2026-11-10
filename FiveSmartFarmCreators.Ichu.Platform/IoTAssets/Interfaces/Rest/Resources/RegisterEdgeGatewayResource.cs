using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

public sealed record RegisterEdgeGatewayResource([Required, StringLength(80)] string Name);
