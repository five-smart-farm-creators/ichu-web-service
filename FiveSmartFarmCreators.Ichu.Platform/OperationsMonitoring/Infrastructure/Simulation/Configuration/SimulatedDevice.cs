namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Simulation.Configuration;

/// <summary>
///     Development stand-in for a collar of IoT Assets and its current assignment.
/// </summary>
public class SimulatedDevice
{
    /// <summary>Gets or sets the device identifier.</summary>
    public Guid DeviceId { get; set; }

    /// <summary>Gets or sets the ranch that owns the device.</summary>
    public Guid RanchId { get; set; }

    /// <summary>Gets or sets the animal assigned to the device, if any.</summary>
    public Guid? CattleId { get; set; }

    /// <summary>Gets or sets the instant the animal was assigned; earlier readings are not attributed to it.</summary>
    public DateTimeOffset AssignedAt { get; set; }
}
