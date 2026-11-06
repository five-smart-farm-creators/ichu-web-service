namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;

/// <summary>
///     Verifies the credentials of the Edge Gateway that sent the current request.
/// </summary>
public interface IEdgeGatewayAuthenticator
{
    /// <summary>
    ///     Authenticates the gateway of the current request asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the verified gateway, or <see langword="null"/> when the credentials are missing or invalid.</returns>
    Task<EdgeGatewayPrincipal?> AuthenticateAsync(CancellationToken cancellationToken);
}
