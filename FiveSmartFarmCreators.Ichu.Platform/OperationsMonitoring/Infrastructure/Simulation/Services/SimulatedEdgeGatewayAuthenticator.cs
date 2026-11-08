using System.Security.Cryptography;
using System.Text;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Simulation.Configuration;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Simulation.Services;

/// <summary>
///     Development-only implementation of <see cref="IEdgeGatewayAuthenticator"/> that accepts the key of a gateway of the "OperationsMonitoring:Simulation" configuration section in the X-Gateway-Key header.
/// </summary>
public class SimulatedEdgeGatewayAuthenticator(
    IHttpContextAccessor httpContextAccessor,
    IOptions<MonitoringSimulationOptions> options) : IEdgeGatewayAuthenticator
{
    /// <summary>Name of the header that carries the development gateway key.</summary>
    public const string KeyHeader = "X-Gateway-Key";

    /// <inheritdoc />
    public Task<EdgeGatewayPrincipal?> AuthenticateAsync(CancellationToken cancellationToken)
    {
        var presentedKey = httpContextAccessor.HttpContext?.Request.Headers[KeyHeader].ToString();
        if (string.IsNullOrEmpty(presentedKey))
        {
            return Task.FromResult<EdgeGatewayPrincipal?>(null);
        }

        // Hashing both keys gives equal lengths, so the comparison time does not reveal how much of a key matched.
        var presentedHash = SHA256.HashData(Encoding.UTF8.GetBytes(presentedKey));
        EdgeGatewayPrincipal? principal = null;
        foreach (var gateway in options.Value.Gateways.Where(g => !string.IsNullOrEmpty(g.Key)))
        {
            if (CryptographicOperations.FixedTimeEquals(presentedHash, SHA256.HashData(Encoding.UTF8.GetBytes(gateway.Key))))
            {
                principal ??= new EdgeGatewayPrincipal(gateway.GatewayId, gateway.RanchId);
            }
        }

        return Task.FromResult(principal);
    }
}
