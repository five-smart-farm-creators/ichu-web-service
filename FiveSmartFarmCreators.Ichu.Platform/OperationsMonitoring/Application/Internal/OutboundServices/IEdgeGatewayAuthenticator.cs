namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;

/// <summary>
///     Verifies the credentials of the Edge Gateway that sent the current request.
/// </summary>
public interface IEdgeGatewayAuthenticator
{
    /// <summary>
    ///     Returns the verified gateway, or null when the credentials are missing or invalid.
    /// </summary>
    Task<EdgeGatewayPrincipal?> AuthenticateAsync(CancellationToken cancellationToken);
}
