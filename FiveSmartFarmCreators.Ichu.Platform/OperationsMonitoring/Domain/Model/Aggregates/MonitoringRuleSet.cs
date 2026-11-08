using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

/// <summary>
///     The revision counter of a ranch's thresholds and geofences. Every rule change advances it, so that Edge Gateways can detect new configuration and alerts can reference the revision used to evaluate them.
/// </summary>
public sealed class MonitoringRuleSet : AggregateRoot
{
    // Entity Framework Core materializes rule sets without running the constructor.
    private MonitoringRuleSet()
    {
    }

    /// <summary>
    ///     Starts the rules of a ranch at revision zero. The access check rejects an empty ranch before any rule changes, so this guard reports only programming errors.
    /// </summary>
    public MonitoringRuleSet(Guid ranchId)
    {
        if (ranchId == Guid.Empty)
        {
            throw new ArgumentException("A ranch identifier is required.", nameof(ranchId));
        }

        RanchId = ranchId;
    }

    public Guid RanchId { get; private set; }

    public long Revision { get; private set; }

    public long Advance() => ++Revision;
}
