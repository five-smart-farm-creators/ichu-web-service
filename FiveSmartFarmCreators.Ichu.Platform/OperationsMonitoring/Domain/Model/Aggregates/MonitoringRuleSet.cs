namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

/// <summary>
/// The revision counter of a ranch's thresholds and geofences. Every rule change advances it so that Edge
/// Gateways can detect new configuration and alerts can reference the revision used to evaluate them.
/// </summary>
public sealed class MonitoringRuleSet
{
    public Guid RanchId { get; private set; }
    public long Revision { get; private set; }

    private MonitoringRuleSet()
    {
    }

    public MonitoringRuleSet(Guid ranchId)
    {
        if (ranchId == Guid.Empty)
        {
            throw MonitoringException.InvalidInput("A ranch identifier is required.");
        }

        RanchId = ranchId;
    }

    public long Advance() => ++Revision;
}
