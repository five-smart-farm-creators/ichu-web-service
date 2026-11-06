using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.QueryServices;

/// <summary>
///     Defines authorized analytics read operations.
/// </summary>
public interface IAnalyticsQueryService
{
    /// <summary>
    ///     Returns authorized herd indicators with source coverage information.
    /// </summary>
    /// <param name="query">The ranch and UTC observation window.</param>
    /// <param name="cancellationToken">Cancels source reads and request processing.</param>
    /// <returns>Indicators with source completeness, reading coverage, and unavailability reasons.</returns>
    Task<HerdIndicatorSet> HandleAsync(GetHerdIndicatorsQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns an authorized lot trend for the selected metric and observation window.
    /// </summary>
    /// <param name="query">The ranch, lot, metric, observation window, and absolute deviation threshold.</param>
    /// <param name="cancellationToken">Cancels source reads and request processing.</param>
    /// <returns>The valid trend points, lot average, outliers, and reading coverage.</returns>
    Task<TrendSeries> HandleAsync(GetLotTrendQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns an animal comparison, or its unavailable reason when fewer than five reference animals have valid readings.
    /// </summary>
    /// <param name="query">The animal and its lot reference scope.</param>
    /// <param name="cancellationToken">Cancels source reads and request processing.</param>
    /// <returns>The animal mean and deviation, or the reason a comparison cannot be produced.</returns>
    Task<AnimalComparison> HandleAsync(GetAnimalComparisonQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns an authorized case progression with observation and incubation-period limits.
    /// </summary>
    /// <param name="query">The ranch, observation window, and health condition.</param>
    /// <param name="cancellationToken">Cancels source reads and request processing.</param>
    /// <returns>Daily case onset counts and the assessment supported by the observation window.</returns>
    Task<EpidemiologicalCurve> HandleAsync(GetEpidemiologicalCurveQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns accessible ranch summaries ordered by alert and follow-up priority.
    /// </summary>
    /// <param name="query">The request to list accessible ranch priorities.</param>
    /// <param name="cancellationToken">Cancels source reads and request processing.</param>
    /// <returns>One attention-priority summary per accessible ranch.</returns>
    Task<IReadOnlyList<RanchPortfolioEntry>> HandleAsync(GetRanchPortfolioQuery query, CancellationToken cancellationToken = default);
}
