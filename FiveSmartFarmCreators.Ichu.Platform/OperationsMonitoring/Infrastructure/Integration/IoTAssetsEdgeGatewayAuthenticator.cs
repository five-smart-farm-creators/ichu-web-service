using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Integration;

/// <summary>
///     Reads the gateway key of the request and verifies it through the IoT Assets facade, which owns the gateway keys.
/// </summary>
public class IoTAssetsEdgeGatewayAuthenticator(
    IHttpContextAccessor httpContextAccessor,
    IIoTAssetsContextFacade iotAssets) : IEdgeGatewayAuthenticator
{
    /// <summary>Name of the header that carries the Edge Gateway key.</summary>
    public const string KeyHeader = "X-Gateway-Key";

    /// <inheritdoc />
    public async Task<EdgeGatewayPrincipal?> AuthenticateAsync(CancellationToken cancellationToken)
    {
        var presentedKey = httpContextAccessor.HttpContext?.Request.Headers[KeyHeader].ToString();
        if (string.IsNullOrEmpty(presentedKey))
        {
            return null;
        }

        var gateway = await iotAssets.AuthenticateEdgeGatewayAsync(presentedKey, cancellationToken);
        return gateway is null ? null : new EdgeGatewayPrincipal(gateway.GatewayId, gateway.RanchId);
    }
}
