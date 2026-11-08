namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;

/// <summary>
///     A verified Edge Gateway and the ranch it serves.
/// </summary>
public record EdgeGatewayPrincipal(Guid GatewayId, Guid RanchId);
