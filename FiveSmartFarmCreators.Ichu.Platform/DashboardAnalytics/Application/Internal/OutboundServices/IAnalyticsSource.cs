using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.OutboundServices;

public interface IAnalyticsSource
{
    Task<IReadOnlyList<RanchPortfolioEntry>> ReadPortfolioAsync(IReadOnlyList<Guid> ranchIds, CancellationToken cancellationToken);
    Task<AnalyticsSnapshot> ReadAsync(Guid ranchId, DateRange period, CancellationToken cancellationToken);
}
