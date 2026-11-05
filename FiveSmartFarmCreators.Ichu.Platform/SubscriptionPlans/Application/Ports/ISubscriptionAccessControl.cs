namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Ports;

/// <summary>
/// Authorization required by Subscription Plans, implemented by the host with the IAM context. Managing a
/// subscription is reserved to a verified account that controls the ranch.
/// </summary>
public interface ISubscriptionAccessControl
{
    /// <summary>Returns the verified account of the current request, or null when no verified identity exists.</summary>
    Task<Guid?> GetCurrentAccountIdAsync(CancellationToken cancellationToken);

    Task<bool> CanAccessRanchAsync(Guid accountId, Guid ranchId, SubscriptionPermission permission, CancellationToken cancellationToken);
}
