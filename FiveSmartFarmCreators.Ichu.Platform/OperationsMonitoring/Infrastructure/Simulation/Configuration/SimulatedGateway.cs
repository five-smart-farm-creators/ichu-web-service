namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Simulation.Configuration;

/// <summary>
///     Development stand-in for an Edge Gateway of IoT Assets and its credential.
/// </summary>
public class SimulatedGateway
{
    public Guid GatewayId { get; set; }

    public Guid RanchId { get; set; }

    public string Key { get; set; } = string.Empty;
}
