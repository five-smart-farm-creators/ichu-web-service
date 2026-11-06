namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Simulation.Configuration;

/// <summary>
///     Development stand-in for a lot of Cattle Information.
/// </summary>
public class SimulatedLot
{
    /// <summary>Gets or sets the lot identifier.</summary>
    public Guid LotId { get; set; }

    /// <summary>Gets or sets the ranch that owns the lot.</summary>
    public Guid RanchId { get; set; }
}
