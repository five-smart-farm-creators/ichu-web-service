namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Ports;

/// <summary>Verifies the credentials of the Edge Gateway that sent the current request.</summary>
public interface IEdgeGatewayAuthenticator
{
    /// <summary>Returns the verified gateway and the ranch it serves, or null when the credentials are missing or invalid.</summary>
    Task<EdgeGatewayPrincipal?> AuthenticateAsync(CancellationToken cancellationToken);
}
