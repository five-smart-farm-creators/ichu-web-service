using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.CommandServices;

/// <summary>
///     Carries the presented key with the new gateway, because the secret cannot be recovered after registration.
/// </summary>
public sealed record EdgeGatewayRegistration(EdgeGateway Gateway, string Key);
