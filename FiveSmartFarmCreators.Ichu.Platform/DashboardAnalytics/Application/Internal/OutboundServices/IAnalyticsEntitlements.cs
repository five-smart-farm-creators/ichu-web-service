namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.OutboundServices;

/// <summary>
///     Checks report export entitlements through the subscription integration boundary.
/// </summary>
public interface IAnalyticsEntitlements
{
    /// <summary>
    ///     Determines whether the ranch subscription permits executive report exports.
    /// </summary>
    Task<bool> CanExportAsync(Guid ranchId, CancellationToken cancellationToken);
}
