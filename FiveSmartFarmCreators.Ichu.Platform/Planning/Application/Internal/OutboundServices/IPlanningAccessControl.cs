namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;

/// <summary>
/// Authorization required by Planning. The host implements it with the IAM context, and every decision is taken for the operation instead of trusting permissions sent by the client.
/// </summary>
public interface IPlanningAccessControl
{
    /// <summary>
    /// Returns the verified account of the current request, or null when the request has none.
    /// </summary>
    Task<Guid?> GetCurrentAccountIdAsync(CancellationToken cancellationToken);

    Task<bool> CanAccessRanchAsync(Guid accountId, Guid ranchId, PlanningPermission permission, CancellationToken cancellationToken);
}
