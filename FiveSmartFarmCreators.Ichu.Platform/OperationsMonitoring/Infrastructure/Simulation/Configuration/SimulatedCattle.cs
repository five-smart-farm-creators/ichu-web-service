namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Simulation.Configuration;

/// <summary>
///     Development stand-in for an animal of Cattle Information.
/// </summary>
public class SimulatedCattle
{
    public Guid CattleId { get; set; }

    public Guid RanchId { get; set; }

    public DateTimeOffset IncorporatedAt { get; set; }

    public DateTimeOffset? DeactivatedAt { get; set; }
}
