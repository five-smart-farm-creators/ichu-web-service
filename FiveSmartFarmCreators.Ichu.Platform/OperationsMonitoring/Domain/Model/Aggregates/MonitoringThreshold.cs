using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

/// <summary>
///     Aggregate root representing the normal range configured by a ranch for one metric and production stage. ICHU
///     does not supply default clinical ranges; an animal whose stage has no configured threshold cannot produce a
///     threshold alert.
/// </summary>
public class MonitoringThreshold
{
    /// <summary>Maximum number of characters of a life stage name.</summary>
    public const int MaximumLifeStageLength = 40;

    private MonitoringThreshold()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MonitoringThreshold"/> class.
    /// </summary>
    /// <param name="ranchId">The ranch that configures the threshold.</param>
    /// <param name="lifeStage">The production stage the threshold applies to.</param>
    /// <param name="metric">The metric the threshold applies to.</param>
    /// <param name="range">The normal range of the metric.</param>
    /// <param name="severity">The severity of the alerts raised when the range is exceeded.</param>
    /// <param name="revision">The rule revision that published the threshold.</param>
    /// <exception cref="ArgumentException">Thrown when the ranch identifier is empty or the life stage is invalid.</exception>
    public MonitoringThreshold(Guid ranchId, string lifeStage, MonitoredMetric metric, ThresholdRange range, AlertSeverity severity, long revision)
    {
        if (ranchId == Guid.Empty)
        {
            throw new ArgumentException("A ranch identifier is required.");
        }

        Id = Guid.NewGuid();
        RanchId = ranchId;
        LifeStage = NormalizeLifeStage(lifeStage);
        Metric = metric;
        Reconfigure(range, severity, revision);
    }

    /// <summary>Gets the identifier of the threshold.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the ranch that configured the threshold.</summary>
    public Guid RanchId { get; private set; }

    /// <summary>Gets the production stage the threshold applies to.</summary>
    public string LifeStage { get; private set; } = string.Empty;

    /// <summary>Gets the metric the threshold applies to.</summary>
    public MonitoredMetric Metric { get; private set; }

    /// <summary>Gets the normal range of the metric.</summary>
    public ThresholdRange Range { get; private set; } = null!;

    /// <summary>Gets the severity of the alerts raised when the range is exceeded.</summary>
    public AlertSeverity Severity { get; private set; }

    /// <summary>Gets the rule revision that last changed the threshold.</summary>
    public long Revision { get; private set; }

    /// <summary>
    ///     Trims a life stage name and checks its length.
    /// </summary>
    /// <param name="lifeStage">The life stage name.</param>
    /// <returns>The trimmed life stage name.</returns>
    /// <exception cref="ArgumentException">Thrown when the name is empty or too long.</exception>
    public static string NormalizeLifeStage(string lifeStage)
    {
        var normalized = lifeStage.Trim();
        if (normalized.Length == 0 || normalized.Length > MaximumLifeStageLength)
        {
            throw new ArgumentException($"A life stage must contain 1 to {MaximumLifeStageLength} characters.");
        }

        return normalized;
    }

    /// <summary>
    ///     Replaces the range and severity of the threshold.
    /// </summary>
    /// <param name="range">The new normal range.</param>
    /// <param name="severity">The new severity.</param>
    /// <param name="revision">The rule revision that publishes the change.</param>
    public void Reconfigure(ThresholdRange range, AlertSeverity severity, long revision)
    {
        Range = range;
        Severity = severity;
        Revision = revision;
    }
}
