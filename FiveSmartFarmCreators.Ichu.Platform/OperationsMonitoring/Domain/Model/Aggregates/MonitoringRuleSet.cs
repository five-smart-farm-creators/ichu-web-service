namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

/// <summary>
///     Aggregate root representing the revision counter of a ranch's thresholds and geofences. Every rule change
///     advances it so that Edge Gateways can detect new configuration and alerts can reference the revision used
///     to evaluate them.
/// </summary>
public sealed class MonitoringRuleSet
{
    private MonitoringRuleSet()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MonitoringRuleSet"/> class at revision zero.
    /// </summary>
    /// <param name="ranchId">The ranch that owns the rules.</param>
    /// <exception cref="ArgumentException">Thrown when the ranch identifier is empty.</exception>
    public MonitoringRuleSet(Guid ranchId)
    {
        if (ranchId == Guid.Empty)
        {
            throw new ArgumentException("A ranch identifier is required.");
        }

        RanchId = ranchId;
    }

    /// <summary>Gets the ranch that owns the rules.</summary>
    public Guid RanchId { get; private set; }

    /// <summary>Gets the current revision of the ranch rules.</summary>
    public long Revision { get; private set; }

    /// <summary>
    ///     Advances the revision after a rule change.
    /// </summary>
    /// <returns>The new revision.</returns>
    public long Advance() => ++Revision;
}
