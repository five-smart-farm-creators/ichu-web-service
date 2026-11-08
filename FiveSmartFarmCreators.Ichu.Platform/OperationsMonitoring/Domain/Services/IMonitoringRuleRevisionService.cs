namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Services;

/// <summary>
///     Advances the rule revision of a ranch whenever a threshold or geofence changes.
/// </summary>
public interface IMonitoringRuleRevisionService
{
    /// <summary>
    ///     Advances the rule revision of a ranch, creating its rule set the first time, and returns the new revision that the changed rule carries.
    /// </summary>
    Task<long> AdvanceAsync(Guid ranchId, CancellationToken cancellationToken = default);
}
