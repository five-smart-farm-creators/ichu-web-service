namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Simulation.Configuration;

/// <summary>
///     Development-only data bound from the "OperationsMonitoring:Simulation" configuration section. It replaces Cattle Information and IoT Assets until those modules exist.
/// </summary>
public class MonitoringSimulationOptions
{
    /// <summary>Name of the configuration section the options are bound from.</summary>
    public const string SectionName = "OperationsMonitoring:Simulation";

    public List<SimulatedLot> Lots { get; set; } = [];

    public List<SimulatedCattle> Cattle { get; set; } = [];

    public List<SimulatedDevice> Devices { get; set; } = [];

    public List<SimulatedGateway> Gateways { get; set; } = [];
}
