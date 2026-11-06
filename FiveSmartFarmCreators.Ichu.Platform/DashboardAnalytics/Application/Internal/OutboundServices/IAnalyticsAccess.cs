namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.OutboundServices;

/// <summary>
///     Checks ranch access without coupling analytics to another context implementation.
/// </summary>
public interface IAnalyticsAccess
{
    /// <summary>
    ///     Lists ranch identifiers accessible to the current account.
    /// </summary>
    Task<IReadOnlyList<Guid>> GetRanchIdsAsync(CancellationToken cancellationToken);
    /// <summary>
    ///     Rejects requests when the current account cannot read the ranch.
    /// </summary>
    Task EnsureReadAsync(Guid ranchId, CancellationToken cancellationToken);
}
