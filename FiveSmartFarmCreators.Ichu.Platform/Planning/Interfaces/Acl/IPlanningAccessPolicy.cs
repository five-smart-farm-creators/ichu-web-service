namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Acl;

/// <summary>
/// Checks Planning permissions through the Iam bounded context.
/// </summary>
public interface IPlanningAccessPolicy
{
    /// <summary>
    /// Checks whether an account may manage Planning data for a ranch.
    /// </summary>
    /// <param name="accountId">The authenticated account identifier.</param>
    /// <param name="ranchId">The ranch identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True when the account may manage Planning data for the ranch.</returns>
    Task<bool> CanManageRanchAsync(Guid accountId, Guid ranchId, CancellationToken cancellationToken);
}
