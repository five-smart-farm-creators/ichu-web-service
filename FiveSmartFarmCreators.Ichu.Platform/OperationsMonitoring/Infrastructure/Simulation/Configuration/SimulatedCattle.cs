namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Simulation.Configuration;

/// <summary>
///     Development stand-in for an animal of Cattle Information.
/// </summary>
public class SimulatedCattle
{
    /// <summary>Gets or sets the animal identifier.</summary>
    public Guid CattleId { get; set; }

    /// <summary>Gets or sets the ranch that owns the animal.</summary>
    public Guid RanchId { get; set; }

    /// <summary>Gets or sets the instant the animal joined the herd.</summary>
    public DateTimeOffset IncorporatedAt { get; set; }

    /// <summary>Gets or sets the instant the animal left the herd, if it did.</summary>
    public DateTimeOffset? DeactivatedAt { get; set; }
}
