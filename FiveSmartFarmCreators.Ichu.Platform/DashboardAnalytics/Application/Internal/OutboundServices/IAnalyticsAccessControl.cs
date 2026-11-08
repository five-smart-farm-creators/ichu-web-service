namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.OutboundServices;

/// <summary>
///     Decides every analytics read from the verified caller, never from permissions sent by the client.
/// </summary>
public interface IAnalyticsAccessControl
{
    Task<Guid?> GetCurrentAccountIdAsync(CancellationToken cancellationToken);

    Task<bool> CanReadRanchAsync(Guid accountId, Guid ranchId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> GetAccessibleRanchIdsAsync(Guid accountId, CancellationToken cancellationToken);
}
