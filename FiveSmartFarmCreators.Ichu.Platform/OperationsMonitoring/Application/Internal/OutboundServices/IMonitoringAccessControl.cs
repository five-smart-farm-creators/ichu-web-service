namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;

/// <summary>
///     Authorization required by the context. The host implements it with the IAM context, and every decision is evaluated for the operation instead of trusting permissions that a client sends.
/// </summary>
public interface IMonitoringAccessControl
{
    /// <summary>
    ///     Returns the verified account of the current request, or null when no verified identity exists.
    /// </summary>
    Task<Guid?> GetCurrentAccountIdAsync(CancellationToken cancellationToken);

    Task<bool> CanAccessRanchAsync(Guid accountId, Guid ranchId, MonitoringPermission permission, CancellationToken cancellationToken);
}
