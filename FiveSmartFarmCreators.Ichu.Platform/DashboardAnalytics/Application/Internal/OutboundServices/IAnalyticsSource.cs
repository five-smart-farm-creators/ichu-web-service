using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.OutboundServices;

/// <summary>
///     Reads analytics observations through external context adapters without owning their persistence.
/// </summary>
public interface IAnalyticsSource
{
    /// <summary>
    ///     Reads one portfolio snapshot for each requested ranch.
    /// </summary>
    Task<IReadOnlyList<RanchPortfolioEntry>> ReadPortfolioAsync(IReadOnlyList<Guid> ranchIds, CancellationToken cancellationToken);
    /// <summary>
    ///     Reads a ranch observation window with explicit source completeness and freshness.
    /// </summary>
    Task<AnalyticsSnapshot> ReadAsync(Guid ranchId, DateRange period, CancellationToken cancellationToken);
}
