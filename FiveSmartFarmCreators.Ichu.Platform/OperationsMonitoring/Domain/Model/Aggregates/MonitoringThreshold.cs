using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

/// <summary>
///     The normal range that a ranch configures for one metric, production stage and severity. ICHU supplies no default clinical ranges, so an animal whose stage has no configured threshold cannot produce a threshold alert.
/// </summary>
public sealed class MonitoringThreshold : AggregateRoot
{
    public const int MaximumLifeStageLength = 40;

    // Entity Framework Core materializes thresholds without running the factory.
    private MonitoringThreshold()
    {
    }

    public MonitoringThresholdId Id { get; private set; }

    public Guid RanchId { get; private set; }

    public string LifeStage { get; private set; } = string.Empty;

    public MonitoredMetric Metric { get; private set; }

    public ThresholdRange Range { get; private set; } = null!;

    public AlertSeverity Severity { get; private set; }

    public long Revision { get; private set; }

    public static Result<MonitoringThreshold> Create(
        Guid ranchId,
        string lifeStage,
        MonitoredMetric metric,
        ThresholdRange range,
        AlertSeverity severity,
        long revision)
    {
        if (ranchId == Guid.Empty)
        {
            return Result<MonitoringThreshold>.Failure(OperationsMonitoringErrors.RanchIdentifierRequired);
        }

        var normalizedLifeStage = NormalizeLifeStage(lifeStage);
        if (normalizedLifeStage.IsFailure)
        {
            return Result<MonitoringThreshold>.Failure(normalizedLifeStage.DomainError);
        }

        var threshold = new MonitoringThreshold
        {
            Id = new MonitoringThresholdId(Guid.NewGuid()),
            RanchId = ranchId,
            LifeStage = normalizedLifeStage.Value!,
            Metric = metric
        };
        threshold.Reconfigure(range, severity, revision);
        return Result<MonitoringThreshold>.Success(threshold);
    }

    public static Result<string> NormalizeLifeStage(string lifeStage)
    {
        var normalized = lifeStage.Trim();
        return normalized.Length is 0 or > MaximumLifeStageLength
            ? Result<string>.Failure(OperationsMonitoringErrors.ThresholdLifeStageLength)
            : Result<string>.Success(normalized);
    }

    public void Reconfigure(ThresholdRange range, AlertSeverity severity, long revision)
    {
        Range = range;
        Severity = severity;
        Revision = revision;
    }
}
