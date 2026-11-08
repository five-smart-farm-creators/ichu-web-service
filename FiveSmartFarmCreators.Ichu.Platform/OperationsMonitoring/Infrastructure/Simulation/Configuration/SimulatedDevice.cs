namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Simulation.Configuration;

/// <summary>
///     Development stand-in for a collar of IoT Assets and its current assignment.
/// </summary>
public class SimulatedDevice
{
    public Guid DeviceId { get; set; }

    public Guid RanchId { get; set; }

    public Guid? CattleId { get; set; }

    public DateTimeOffset AssignedAt { get; set; }
}
