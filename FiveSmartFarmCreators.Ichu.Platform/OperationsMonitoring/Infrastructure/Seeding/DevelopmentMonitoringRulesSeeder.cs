using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Seeding;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Seeding;

/// <summary>
///     Configures demonstration thresholds for the Development life stages and a geofence for the Development lot, so
///     that the edge raises alerts. The values are placeholders for demos, not validated clinical ranges.
/// </summary>
public sealed class DevelopmentMonitoringRulesSeeder(
    IMonitoringThresholdRepository thresholds,
    IGeofenceRepository geofences,
    IMonitoringRuleRevisionService ruleRevisionService,
    IOperationsMonitoringUnitOfWork unitOfWork)
{
    // Life stage names as Cattle Information reports them to the edge.
    private static readonly string[] LifeStages = ["MilkingCow", "Heifer"];

    private static readonly DevelopmentThreshold[] Thresholds =
    [
        new(MonitoredMetric.BodyTemperature, 37.8, 39.5, AlertSeverity.Critical),
        new(MonitoredMetric.ActivityIndex, 15, 85, AlertSeverity.Warning),
        new(MonitoredMetric.HeartRate, 48, 84, AlertSeverity.Warning),
        new(MonitoredMetric.HeartRate, 40, 90, AlertSeverity.Critical),
        new(MonitoredMetric.MethaneIndex, 0, 60, AlertSeverity.Warning),
        new(MonitoredMetric.MethaneIndex, 0, 80, AlertSeverity.Critical)
    ];

    // A square of about 1.1 km around the position used in the edge examples.
    private static readonly (double Latitude, double Longitude)[] PastureBoundary =
    [
        (-16.505, -68.105), (-16.505, -68.095), (-16.495, -68.095), (-16.495, -68.105)
    ];

    /// <summary>
    ///     Skips the access check because no HTTP account exists at startup. Missing rules are added under one revision,
    ///     since the revision service creates the rule set on first use and must run once per unit of work.
    /// </summary>
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var missingThresholds = new List<(string LifeStage, DevelopmentThreshold Rule)>();
        foreach (var lifeStage in LifeStages)
        {
            foreach (var rule in Thresholds)
            {
                if (await thresholds.FindByRanchLifeStageAndMetricAndSeverityAsync(DevelopmentSeed.RanchId, lifeStage, rule.Metric, rule.Severity, cancellationToken) is null)
                {
                    missingThresholds.Add((lifeStage, rule));
                }
            }
        }

        var geofenceMissing = await geofences.FindByLotAsync(DevelopmentSeed.RanchId, DevelopmentSeed.LotId, cancellationToken) is null;
        if (missingThresholds.Count == 0 && !geofenceMissing)
        {
            return;
        }

        var revision = await ruleRevisionService.AdvanceAsync(DevelopmentSeed.RanchId, cancellationToken);
        foreach (var (lifeStage, rule) in missingThresholds)
        {
            await thresholds.AddAsync(Unwrap(MonitoringThreshold.Create(DevelopmentSeed.RanchId, lifeStage, rule.Metric,
                new ThresholdRange(rule.MinValue, rule.MaxValue), rule.Severity, revision)), cancellationToken);
        }

        if (geofenceMissing)
        {
            var boundary = PastureBoundary
                .Select(vertex => Unwrap(GeoCoordinate.Create(vertex.Latitude, vertex.Longitude)))
                .ToList();
            await geofences.AddAsync(Unwrap(Geofence.Create(DevelopmentSeed.RanchId, DevelopmentSeed.LotId,
                "Development pasture", 25, AlertSeverity.Warning, boundary, revision)), cancellationToken);
        }

        if (!await unitOfWork.TryCompleteAsync(cancellationToken))
        {
            throw new InvalidOperationException("The Development monitoring rules could not be saved.");
        }
    }

    private static T Unwrap<T>(Result<T> result) =>
        result.IsFailure ? throw new InvalidOperationException(result.DomainError.Message) : result.Value!;

    private sealed record DevelopmentThreshold(MonitoredMetric Metric, double MinValue, double MaxValue, AlertSeverity Severity);
}
