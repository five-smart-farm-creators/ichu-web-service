namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices.ACL;

/// <summary>
///     A verified Edge Gateway and the ranch it serves.
/// </summary>
/// <param name="GatewayId">The gateway identifier.</param>
/// <param name="RanchId">The ranch the gateway serves.</param>
public sealed record EdgeGatewayPrincipal(Guid GatewayId, Guid RanchId);
