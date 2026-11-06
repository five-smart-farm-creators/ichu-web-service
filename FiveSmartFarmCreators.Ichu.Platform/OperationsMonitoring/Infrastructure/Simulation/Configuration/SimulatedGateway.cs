namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Simulation.Configuration;

/// <summary>
///     Development stand-in for an Edge Gateway of IoT Assets and its credential.
/// </summary>
public class SimulatedGateway
{
    /// <summary>Gets or sets the gateway identifier.</summary>
    public Guid GatewayId { get; set; }

    /// <summary>Gets or sets the ranch the gateway serves.</summary>
    public Guid RanchId { get; set; }

    /// <summary>Gets or sets the development key the gateway sends in the X-Gateway-Key header.</summary>
    public string Key { get; set; } = string.Empty;
}
