using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Services;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.QueryServices;

/// <summary>
///     Coordinates access checks, source reads, and analytics calculations.
/// </summary>
public sealed class AnalyticsQueryService(IAnalyticsAccess access, IAnalyticsSource source, AnalyticsDomainService domain) : IAnalyticsQueryService
{
    /// <inheritdoc />
    public async Task<HerdIndicatorSet> HandleAsync(GetHerdIndicatorsQuery query,
        CancellationToken cancellationToken = default) =>
        domain.BuildIndicators(await ReadAsync(query.RanchId, query.Period, cancellationToken));

    /// <inheritdoc />
    public async Task<TrendSeries> HandleAsync(GetLotTrendQuery query,
        CancellationToken cancellationToken = default) =>
        domain.BuildTrend(await ReadAsync(query.RanchId, query.Period, cancellationToken), query.LotId, query.Metric, query.OutlierDeviation);

    /// <inheritdoc />
    public async Task<AnimalComparison> HandleAsync(GetAnimalComparisonQuery query,
        CancellationToken cancellationToken = default) =>
        domain.CompareAgainstLot(await HandleAsync(new GetLotTrendQuery(query.RanchId, query.LotId, query.Period,
            query.Metric, query.OutlierDeviation), cancellationToken), query.AnimalId);

    /// <inheritdoc />
    public async Task<EpidemiologicalCurve> HandleAsync(GetEpidemiologicalCurveQuery query,
        CancellationToken cancellationToken = default) =>
        domain.BuildCurve(await ReadAsync(query.RanchId, query.Period, cancellationToken), query.Condition);

    /// <inheritdoc />
    public async Task<IReadOnlyList<RanchPortfolioEntry>> HandleAsync(GetRanchPortfolioQuery query,
        CancellationToken cancellationToken = default)
    {
        var ids = await access.GetRanchIdsAsync(cancellationToken);
        if (ids.Count == 0)
        {
            return [];
        }

        var portfolio = await source.ReadPortfolioAsync(ids, cancellationToken);
        if (portfolio.Count != ids.Count || portfolio.DistinctBy(r => r.RanchId).Count() != portfolio.Count || portfolio.Any(r => !ids.Contains(r.RanchId) ||
            r.CriticalAlerts < 0 || r.WarningAlerts < 0 || r.InformativeAlerts < 0 || r.UnresponsiveTreatmentAnimals < 0 ||
            r.OverdueCampaigns < 0 || string.IsNullOrWhiteSpace(r.Source)))
        {
            throw new AnalyticsException("unavailable", "The portfolio source returned an inconsistent snapshot.");
        }

        return portfolio.OrderByDescending(r => r.CriticalAlerts).ThenByDescending(r => r.WarningAlerts)
            .ThenByDescending(r => r.UnresponsiveTreatmentAnimals).ThenByDescending(r => r.OverdueCampaigns).ToList();
    }

    private async Task<AnalyticsSnapshot> ReadAsync(Guid ranchId, DateRange period, CancellationToken cancellationToken)
    {
        if (ranchId == Guid.Empty)
        {
            throw new ArgumentException("A ranch identifier is required.");
        }

        await access.EnsureReadAsync(ranchId, cancellationToken);
        var snapshot = await source.ReadAsync(ranchId, period, cancellationToken);
        var ids = snapshot.Animals.Select(a => a.AnimalId).ToHashSet();
        if (snapshot.RanchId != ranchId || snapshot.Period != period || string.IsNullOrWhiteSpace(snapshot.Source) ||
            ids.Count != snapshot.Animals.Count || snapshot.ExpectedReadings < 0 || snapshot.UpdatedAt < period.To ||
            snapshot.BirthCount < 0 || snapshot.ConfirmedPregnancies < 0 || ids.Contains(Guid.Empty) || snapshot.Readings.Any(r => !period.Contains(r.RecordedAt)) ||
            snapshot.Readings.Any(r => !ids.Contains(r.AnimalId)) || snapshot.Alerts.Any(a => !ids.Contains(a.AnimalId)) ||
            snapshot.Cases.Any(c => !ids.Contains(c.AnimalId)) || snapshot.Animals.Any(a => a.ExpectedReadings < 0) ||
            snapshot.Readings.DistinctBy(r => (r.AnimalId, r.RecordedAt)).Count() != snapshot.Readings.Count ||
            snapshot.Alerts.DistinctBy(a => a.AlertId).Count() != snapshot.Alerts.Count ||
            snapshot.Cases.DistinctBy(c => c.CaseId).Count() != snapshot.Cases.Count)
        {
            throw new AnalyticsException("unavailable", "The analytics source returned an inconsistent snapshot.");
        }

        return snapshot;
    }
}
