namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.OutboundServices;

public interface IAnalyticsEntitlements
{
    Task<bool> CanExportAsync(Guid ranchId, CancellationToken cancellationToken);
}
