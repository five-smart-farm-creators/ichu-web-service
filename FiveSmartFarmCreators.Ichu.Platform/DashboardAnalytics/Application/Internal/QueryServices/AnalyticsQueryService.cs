using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.QueryServices;

/// <summary>
///     Checks access and source consistency before any indicator is calculated, so a caller never receives data it may not read.
/// </summary>
public sealed class AnalyticsQueryService(IAnalyticsAccessControl access, IAnalyticsSource source, AnalyticsDomainService domain) : IAnalyticsQueryService
{
    /// <inheritdoc />
    public async Task<Result<HerdIndicatorSet>> HandleAsync(GetHerdIndicatorsQuery query, CancellationToken cancellationToken = default)
    {
        var snapshot = await ReadAsync(query.RanchId, query.Period, cancellationToken);
        return snapshot.IsSuccess
            ? Result<HerdIndicatorSet>.Success(domain.BuildIndicators(snapshot.Value!))
            : Result<HerdIndicatorSet>.Failure(snapshot.DomainError);
    }

    /// <inheritdoc />
    public async Task<Result<TrendSeries>> HandleAsync(GetLotTrendQuery query, CancellationToken cancellationToken = default)
    {
        var snapshot = await ReadAsync(query.RanchId, query.Period, cancellationToken);
        return snapshot.IsSuccess
            ? domain.BuildTrend(snapshot.Value!, query.LotId, query.Metric, query.OutlierDeviation)
            : Result<TrendSeries>.Failure(snapshot.DomainError);
    }

    /// <inheritdoc />
    public async Task<Result<AnimalComparison>> HandleAsync(GetAnimalComparisonQuery query, CancellationToken cancellationToken = default)
    {
        var trend = await HandleAsync(new GetLotTrendQuery(query.RanchId, query.LotId, query.Period, query.Metric, query.OutlierDeviation),
            cancellationToken);
        return trend.IsSuccess
            ? Result<AnimalComparison>.Success(domain.CompareAgainstLot(trend.Value!, query.AnimalId))
            : Result<AnimalComparison>.Failure(trend.DomainError);
    }

    /// <inheritdoc />
    public async Task<Result<EpidemiologicalCurve>> HandleAsync(GetEpidemiologicalCurveQuery query, CancellationToken cancellationToken = default)
    {
        var snapshot = await ReadAsync(query.RanchId, query.Period, cancellationToken);
        return snapshot.IsSuccess
            ? domain.BuildCurve(snapshot.Value!, query.Condition)
            : Result<EpidemiologicalCurve>.Failure(snapshot.DomainError);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<RanchPortfolioEntry>>> HandleAsync(GetRanchPortfolioQuery query, CancellationToken cancellationToken = default)
    {
        var accountId = await access.GetCurrentAccountIdAsync(cancellationToken);
        if (accountId is not { } verifiedAccountId)
        {
            return Result<IReadOnlyList<RanchPortfolioEntry>>.Failure(DashboardAnalyticsErrors.RanchAccessDenied);
        }

        var ids = await access.GetAccessibleRanchIdsAsync(verifiedAccountId, cancellationToken);
        if (ids.Count == 0)
        {
            return Result<IReadOnlyList<RanchPortfolioEntry>>.Success([]);
        }

        var portfolio = await source.ReadPortfolioAsync(ids, cancellationToken);
        if (!IsPortfolioConsistent(portfolio, ids))
        {
            return Result<IReadOnlyList<RanchPortfolioEntry>>.Failure(DashboardAnalyticsErrors.SourceUnavailable);
        }

        return Result<IReadOnlyList<RanchPortfolioEntry>>.Success(portfolio
            .OrderByDescending(r => r.CriticalAlerts).ThenByDescending(r => r.WarningAlerts)
            .ThenByDescending(r => r.UnresponsiveTreatmentAnimals).ThenByDescending(r => r.OverdueCampaigns)
            .ToList());
    }

    private async Task<Result<AnalyticsSnapshot>> ReadAsync(Guid ranchId, DateRange period, CancellationToken cancellationToken)
    {
        if (ranchId == Guid.Empty)
        {
            return Result<AnalyticsSnapshot>.Failure(DashboardAnalyticsErrors.RanchRequired);
        }

        var accountId = await access.GetCurrentAccountIdAsync(cancellationToken);
        if (accountId is not { } verifiedAccountId || !await access.CanReadRanchAsync(verifiedAccountId, ranchId, cancellationToken))
        {
            return Result<AnalyticsSnapshot>.Failure(DashboardAnalyticsErrors.RanchAccessDenied);
        }

        var snapshot = await source.ReadAsync(ranchId, period, cancellationToken);
        return IsSnapshotConsistent(snapshot, ranchId, period)
            ? Result<AnalyticsSnapshot>.Success(snapshot)
            : Result<AnalyticsSnapshot>.Failure(DashboardAnalyticsErrors.SourceUnavailable);
    }

    // Unavailable counts are null, so they are compared with "is not < 0": a null comparison would reject them.
    private static bool IsPortfolioConsistent(IReadOnlyList<RanchPortfolioEntry> portfolio, IReadOnlyList<Guid> ids) =>
        portfolio.Count == ids.Count && portfolio.DistinctBy(r => r.RanchId).Count() == portfolio.Count &&
        portfolio.All(r => ids.Contains(r.RanchId) && r.CriticalAlerts >= 0 && r.WarningAlerts >= 0 && r.InformativeAlerts >= 0 &&
            r.UnresponsiveTreatmentAnimals is not < 0 && r.OverdueCampaigns is not < 0 && !string.IsNullOrWhiteSpace(r.Source));

    private static bool IsSnapshotConsistent(AnalyticsSnapshot snapshot, Guid ranchId, DateRange period)
    {
        var ids = snapshot.Animals.Select(a => a.AnimalId).ToHashSet();
        return snapshot.RanchId == ranchId && snapshot.Period == period && !string.IsNullOrWhiteSpace(snapshot.Source) &&
            ids.Count == snapshot.Animals.Count && !ids.Contains(Guid.Empty) &&
            snapshot.ExpectedReadings is not < 0 && snapshot.BirthCount is not < 0 && snapshot.ConfirmedPregnancies is not < 0 &&
            snapshot.Animals.All(a => a.ExpectedReadings is not < 0) &&
            snapshot.Readings.All(r => period.Contains(r.RecordedAt) && ids.Contains(r.AnimalId)) &&
            snapshot.Readings.DistinctBy(r => (r.AnimalId, r.RecordedAt)).Count() == snapshot.Readings.Count &&
            snapshot.Alerts.All(a => ids.Contains(a.AnimalId)) &&
            snapshot.Alerts.DistinctBy(a => a.AlertId).Count() == snapshot.Alerts.Count &&
            snapshot.Cases.All(c => ids.Contains(c.AnimalId)) &&
            snapshot.Cases.DistinctBy(c => c.CaseId).Count() == snapshot.Cases.Count;
    }
}
