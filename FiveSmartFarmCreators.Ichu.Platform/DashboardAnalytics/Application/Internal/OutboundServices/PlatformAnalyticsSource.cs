using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.OutboundServices;

/// <summary>
///     Builds analytics snapshots from the facades of other contexts. Their enums become analytics vocabulary here, so the domain never depends on them.
/// </summary>
public sealed class PlatformAnalyticsSource(
    ICattleInformationContextFacade cattleInformation,
    IOperationsMonitoringContextFacade operationsMonitoring,
    IOptions<DashboardAnalyticsOptions> options,
    TimeProvider clock) : IAnalyticsSource
{
    private const string SourceName = "ichu-platform";

    /// <inheritdoc />
    public async Task<IReadOnlyList<RanchPortfolioEntry>> ReadPortfolioAsync(IReadOnlyList<Guid> ranchIds, CancellationToken cancellationToken)
    {
        var updatedAt = clock.GetUtcNow();
        var entries = new List<RanchPortfolioEntry>(ranchIds.Count);
        foreach (var ranchId in ranchIds)
        {
            var counts = await operationsMonitoring.GetOpenAlertCountsAsync(ranchId, cancellationToken);
            // Treatment non-response and overdue campaigns have no source yet, so they stay unavailable.
            entries.Add(new RanchPortfolioEntry(ranchId, updatedAt, SourceName, counts.Critical, counts.Warning, counts.Informative,
                UnresponsiveTreatmentAnimals: null, OverdueCampaigns: null));
        }

        return entries;
    }

    /// <inheritdoc />
    public async Task<AnalyticsSnapshot> ReadAsync(Guid ranchId, DateRange period, CancellationToken cancellationToken)
    {
        var fromDate = DateOnly.FromDateTime(period.From.UtcDateTime);
        var toDate = DateOnly.FromDateTime(period.To.UtcDateTime);
        var population = (await cattleInformation.GetHerdAsync(ranchId, cancellationToken))
            .Where(member => IsInPopulation(member, fromDate, toDate))
            .ToList();
        var populationIds = population.Select(member => member.CattleId).ToHashSet();

        var readings = await operationsMonitoring.GetReadingsAsync(ranchId, period.From, period.To, cancellationToken);
        var alerts = await operationsMonitoring.GetAlertsAsync(ranchId, period.From, period.To, cancellationToken);
        var operations = await operationsMonitoring.GetRanchLivestockOperationsAsync(ranchId, period.From, period.To, cancellationToken);

        var alertSnapshots = alerts
            .Where(alert => populationIds.Contains(alert.CattleId))
            .Select(alert => new AlertSnapshot(alert.AlertId, alert.CattleId, alert.Type.ToString().ToLowerInvariant(), alert.RaisedAt,
                ClosedAt(alert)))
            .ToList();
        var outsideGeofence = alertSnapshots
            .Where(alert => alert.Type == "geofence" && alert.RaisedAt < period.To && (alert.ClosedAt is null || alert.ClosedAt >= period.To))
            .Select(alert => alert.AnimalId)
            .ToHashSet();

        var animals = population
            .Select(member => new AnimalSnapshot(member.CattleId, member.LotId ?? Guid.Empty,
                DiedInPeriod: DiedDuring(member, fromDate, toDate), OutsideGeofence: outsideGeofence.Contains(member.CattleId)))
            .ToList();

        // Only one reading per animal and instant counts, so a duplicated capture cannot weigh twice.
        var readingSnapshots = readings
            .Where(reading => populationIds.Contains(reading.CattleId) && period.Contains(reading.RecordedAt))
            .DistinctBy(reading => (reading.CattleId, reading.RecordedAt))
            .Select(reading => new ReadingSnapshot(reading.CattleId, reading.RecordedAt,
                reading.BodyTemperatureCelsius, reading.ActivityIndex, reading.RuminationMinutes))
            .ToList();

        var cases = alertSnapshots
            .Where(alert => (alert.Type is "temperature" or "activity") && period.Contains(alert.RaisedAt))
            .Select(alert => new HealthCaseSnapshot(alert.AlertId, alert.AnimalId, alert.Type, alert.RaisedAt))
            .ToList();

        // Pregnancy confirmations and expected readings have no source yet, so they stay unavailable.
        return new AnalyticsSnapshot(ranchId, period, clock.GetUtcNow(), SourceName,
            PopulationComplete: true, AlertsComplete: true, GeofencesComplete: true, CasesComplete: true,
            options.Value.ConditionIncubationDays, ExpectedReadings: null, animals, readingSnapshots, alertSnapshots, cases)
        {
            BirthCount = operations.Count(operation => operation.Type == OperationType.Birth)
        };
    }

    // Operations and Monitoring keeps an alert open while it is pending and not cleared, so the first of both closes it.
    private static DateTimeOffset? ClosedAt(MonitoringAlertRecord alert) =>
        alert.AcknowledgedAt is { } acknowledgedAt && (alert.ClearedAt is null || acknowledgedAt < alert.ClearedAt)
            ? acknowledgedAt
            : alert.ClearedAt;

    private static bool IsInPopulation(HerdMemberSnapshot member, DateOnly fromDate, DateOnly toDate) =>
        member.BirthDate < toDate && (member.ExitDate is null || member.ExitDate >= fromDate);

    private static bool DiedDuring(HerdMemberSnapshot member, DateOnly fromDate, DateOnly toDate) =>
        member.ExitReason == nameof(ExitReason.Death) && member.ExitDate is { } exitDate && exitDate >= fromDate && exitDate < toDate;
}
