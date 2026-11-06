namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices.ACL;

/// <summary>
///     Authorization required by Operations and Monitoring. The host implements it with the IAM context; decisions
///     are evaluated for every operation and never trust permissions sent by a client.
/// </summary>
public interface IMonitoringAccessControl
{
    /// <summary>
    ///     Gets the verified account of the current request asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the account identifier, or <see langword="null"/> when no verified identity exists.</returns>
    Task<Guid?> GetCurrentAccountIdAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Determines whether an account holds a permission on a ranch asynchronously.
    /// </summary>
    /// <param name="accountId">The verified account.</param>
    /// <param name="ranchId">The ranch the operation targets.</param>
    /// <param name="permission">The required permission.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task containing <see langword="true"/> when the account holds the permission.</returns>
    Task<bool> CanAccessRanchAsync(Guid accountId, Guid ranchId, MonitoringPermission permission, CancellationToken cancellationToken);
}
