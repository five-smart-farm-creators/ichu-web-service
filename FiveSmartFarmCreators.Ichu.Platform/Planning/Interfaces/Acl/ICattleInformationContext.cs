namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Acl;

/// <summary>
/// Reads animal identity and ranch membership through CattleInformation.
/// </summary>
public interface ICattleInformationContext
{
    /// <summary>
    /// Finds the current animal reference without reading CattleInformation tables.
    /// </summary>
    /// <param name="cattleId">The animal identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current reference, or null when the animal does not exist.</returns>
    Task<CattleReference?> FindCattleAsync(Guid cattleId, CancellationToken cancellationToken);
}
