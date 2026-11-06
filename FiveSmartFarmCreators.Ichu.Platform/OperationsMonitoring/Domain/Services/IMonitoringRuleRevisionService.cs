namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Services;

/// <summary>
///     Domain service that advances the rule revision of a ranch whenever a threshold or geofence changes.
/// </summary>
public interface IMonitoringRuleRevisionService
{
    /// <summary>
    ///     Advances the rule revision of a ranch asynchronously, creating its rule set the first time.
    /// </summary>
    /// <param name="ranchId">The ranch whose rules change.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the new revision, which the changed rule must carry.</returns>
    Task<long> AdvanceAsync(Guid ranchId, CancellationToken cancellationToken = default);
}
