namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Acl;

/// <summary>
/// Exposes the authenticated account identifier resolved by Iam.
/// </summary>
public interface ICurrentAccountContext
{
    /// <summary>
    /// Gets the authenticated account identifier for the current request.
    /// </summary>
    /// <returns>The current account identifier.</returns>
    Guid GetRequiredAccountId();
}
