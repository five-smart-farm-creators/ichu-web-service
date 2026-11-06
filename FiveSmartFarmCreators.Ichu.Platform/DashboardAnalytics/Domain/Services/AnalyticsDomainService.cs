using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Services;

/// <summary>
///     Calculates indicators, trends, comparisons, and case progression from supplied observations.
/// </summary>
public sealed class AnalyticsDomainService
{
    /// <summary>
    ///     Calculates period indicators while retaining incomplete-source and reading-coverage reasons.
    /// </summary>
    public HerdIndicatorSet BuildIndicators(AnalyticsSnapshot snapshot)
    {
        var valid = snapshot.Readings.Where(r => snapshot.Period.Contains(r.RecordedAt) &&
            (IsValid(r.BodyTemperature) || IsValid(r.ActivityIndex) || IsValid(r.RuminationMinutes))).ToList();
        var monitoredIds = valid.Select(r => r.AnimalId).ToHashSet();
        var population = snapshot.Animals.Count;
        var monitored = monitoredIds.Count;
        var openAlerts = snapshot.Alerts.Where(a => a.RaisedAt < snapshot.Period.To &&
            (a.ClosedAt is null || a.ClosedAt >= snapshot.Period.To)).GroupBy(a => a.Type)
            .ToDictionary(g => g.Key, g => g.Count());
        return new HerdIndicatorSet(snapshot.RanchId, snapshot.Period, snapshot.UpdatedAt, snapshot.Source,
            new IndicatorValue(snapshot.PopulationComplete ? monitored : null, "animals", "distinct animals with valid period readings", population,
                snapshot.PopulationComplete ? null : "The population snapshot is incomplete."),
            snapshot.AlertsComplete ? openAlerts : null, snapshot.AlertsComplete ? null : "The alert snapshot is incomplete.",
            new IndicatorValue(snapshot.GeofencesComplete && snapshot.PopulationComplete ? snapshot.Animals.Count(a => a.OutsideGeofence) : null,
                "animals", "animals outside their assigned geofence at the period end", population,
                snapshot.GeofencesComplete && snapshot.PopulationComplete ? null : "Geofence observations are incomplete."),
            new IndicatorValue(snapshot.PopulationComplete && population > 0 ? 100.0 * snapshot.Animals.Count(a => a.DiedInPeriod) / population : null,
                "percent", "100 * deaths during period / animals at risk during period", snapshot.PopulationComplete ? population : null,
                !snapshot.PopulationComplete ? "The population snapshot is incomplete." : population == 0 ? "There are no animals at risk." : null),
            new IndicatorValue(snapshot.BirthCount, "events", "recorded births during the period", null,
                snapshot.BirthCount is null ? "Birth observations are unavailable." : null),
            new IndicatorValue(snapshot.ConfirmedPregnancies, "events", "confirmed pregnancies during the period", null,
                snapshot.ConfirmedPregnancies is null ? "Pregnancy observations are unavailable." : null),
            snapshot.Animals.Count(a => !monitoredIds.Contains(a.AnimalId)),
            snapshot.ExpectedReadings is { } expected ? Math.Max(0, expected - valid.Count) : null,
            snapshot.ExpectedReadings is null ? "The expected reading count is unavailable." : null);
    }

    /// <summary>
    ///     Builds an equally weighted lot trend using finite metric readings and an absolute outlier threshold.
    /// </summary>
    public TrendSeries BuildTrend(AnalyticsSnapshot snapshot, Guid lotId, string metric, double outlierDeviation)
    {
        if (lotId == Guid.Empty || !double.IsFinite(outlierDeviation) || outlierDeviation <= 0)
        {
            throw new ArgumentException("A lot and a positive finite outlier deviation are required.");
        }

        var unit = metric switch
        {
            "body-temperature" => "celsius",
            "activity-index" => "index",
            "rumination-minutes" => "minutes",
            _ => throw new ArgumentException("Metric must be body-temperature, activity-index or rumination-minutes.")
        };
        var animals = snapshot.Animals.Where(a => a.LotId == lotId).ToList();
        var ids = animals.Select(a => a.AnimalId).ToHashSet();
        var points = snapshot.Readings.Where(r => ids.Contains(r.AnimalId) && snapshot.Period.Contains(r.RecordedAt))
            .Select(r => (Reading: r, Value: metric switch
            {
                "body-temperature" => r.BodyTemperature,
                "activity-index" => r.ActivityIndex,
                _ => r.RuminationMinutes
            })).Where(p => IsValid(p.Value)).Select(p => new TrendPoint(p.Reading.AnimalId, p.Reading.RecordedAt, p.Value!.Value))
            .OrderBy(p => p.RecordedAt).ToList();
        // Weight each animal equally regardless of how frequently its device reports.
        var averages = points.GroupBy(p => p.AnimalId).Select(g => (Id: g.Key, Average: g.Average(p => p.Value))).ToList();
        double? mean = averages.Count == 0 ? null : averages.Average(a => a.Average);
        var outliers = mean is { } average ? averages.Where(a => Math.Abs(a.Average - average) > outlierDeviation).Select(a => a.Id).ToList() : [];
        return new TrendSeries(snapshot.RanchId, lotId, snapshot.Period, snapshot.UpdatedAt, snapshot.Source, metric, unit,
            mean, outlierDeviation, points, outliers,
            animals.All(a => a.ExpectedReadings.HasValue) ? Math.Max(0, animals.Sum(a => a.ExpectedReadings!.Value) - points.Count) : null,
            animals.Any(a => a.ExpectedReadings is null) ? "Expected readings are unavailable for part of the lot." : null,
            points.Count == 0 ? "There are no valid readings for this lot and metric." : null);
    }

    /// <summary>
    ///     Compares an animal with its lot only when at least five animals have valid readings.
    /// </summary>
    public AnimalComparison CompareAgainstLot(TrendSeries trend, Guid animalId)
    {
        var count = trend.Points.Select(p => p.AnimalId).Distinct().Count();
        var points = trend.Points.Where(p => p.AnimalId == animalId).ToList();
        if (count < 5 || points.Count == 0)
        {
            return new AnimalComparison(animalId, trend with { LotAverage = null, Points = [], OutlierAnimals = [] }, null, null, count,
                count < 5 ? "At least five animals with valid telemetry are required." : "The animal has no valid readings in this lot.");
        }

        var average = points.Average(p => p.Value);
        return new AnimalComparison(animalId, trend, average, average - trend.LotAverage, count, null);
    }

    /// <summary>
    ///     Groups case onset by UTC day and assesses the available incubation observation window.
    /// </summary>
    public EpidemiologicalCurve BuildCurve(AnalyticsSnapshot snapshot, string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            throw new ArgumentException("A condition is required.");
        }

        var incubationDays = snapshot.ConditionIncubationDays.FirstOrDefault(c => c.Key.Equals(condition, StringComparison.OrdinalIgnoreCase)).Value;
        if (!snapshot.CasesComplete || incubationDays <= 0)
        {
            return new EpidemiologicalCurve(snapshot.RanchId, snapshot.Period, snapshot.UpdatedAt, snapshot.Source,
                condition, new Dictionary<DateOnly, int>(), null, null,
                !snapshot.CasesComplete ? "Case observations are incomplete." : "The condition has no configured incubation period.");
        }

        var cases = snapshot.Cases.Where(c => c.Condition.Equals(condition, StringComparison.OrdinalIgnoreCase) && snapshot.Period.Contains(c.Onset))
            .DistinctBy(c => c.CaseId).ToList();
        if (cases.Count < 2)
        {
            return new EpidemiologicalCurve(snapshot.RanchId, snapshot.Period, snapshot.UpdatedAt, snapshot.Source,
                condition, new Dictionary<DateOnly, int>(), null, null, "At least two recorded cases are required to build a progression.");
        }

        var grouped = cases.GroupBy(c => DateOnly.FromDateTime(c.Onset.UtcDateTime)).ToDictionary(g => g.Key, g => g.Count());
        var days = Math.Max(0, (int)(snapshot.Period.To - cases.Max(c => c.Onset)).TotalDays);
        var observationDays = (snapshot.Period.To - snapshot.Period.From).TotalDays;
        return new EpidemiologicalCurve(snapshot.RanchId, snapshot.Period, snapshot.UpdatedAt, snapshot.Source,
            condition, grouped, days, observationDays >= incubationDays ? days >= incubationDays : null,
            observationDays >= incubationDays ? null : "The observation period is shorter than the incubation period.");
    }

    private static bool IsValid(double? value) => value is { } number && double.IsFinite(number);
}
