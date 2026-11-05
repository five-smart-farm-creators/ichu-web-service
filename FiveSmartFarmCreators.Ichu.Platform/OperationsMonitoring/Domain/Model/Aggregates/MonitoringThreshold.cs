using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

/// <summary>
/// The normal range configured by a ranch for one metric and production stage. ICHU does not supply default
/// clinical ranges; an animal whose stage has no configured threshold cannot produce a threshold alert.
/// </summary>
public sealed class MonitoringThreshold
{
    public const int MaximumLifeStageLength = 40;

    public Guid Id { get; private set; }
    public Guid RanchId { get; private set; }
    public string LifeStage { get; private set; } = string.Empty;
    public MonitoredMetric Metric { get; private set; }
    public ThresholdRange Range { get; private set; } = null!;
    public AlertSeverity Severity { get; private set; }
    public long Revision { get; private set; }

    private MonitoringThreshold()
    {
    }

    public MonitoringThreshold(Guid ranchId, string lifeStage, MonitoredMetric metric, ThresholdRange range, AlertSeverity severity, long revision)
    {
        if (ranchId == Guid.Empty)
        {
            throw MonitoringException.InvalidInput("A ranch identifier is required.");
        }

        Id = Guid.NewGuid();
        RanchId = ranchId;
        LifeStage = NormalizeLifeStage(lifeStage);
        Metric = metric;
        Reconfigure(range, severity, revision);
    }

    public void Reconfigure(ThresholdRange range, AlertSeverity severity, long revision)
    {
        Range = range;
        Severity = severity;
        Revision = revision;
    }

    public static string NormalizeLifeStage(string lifeStage)
    {
        var normalized = lifeStage.Trim();
        if (normalized.Length == 0 || normalized.Length > MaximumLifeStageLength)
        {
            throw MonitoringException.InvalidInput($"A life stage must contain 1 to {MaximumLifeStageLength} characters.");
        }

        return normalized;
    }
}
