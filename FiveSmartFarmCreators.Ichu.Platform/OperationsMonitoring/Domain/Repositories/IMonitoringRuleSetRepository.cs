using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

/// <summary>
///     Domain repository interface for managing <see cref="MonitoringRuleSet"/> aggregates.
/// </summary>
public interface IMonitoringRuleSetRepository
{
    /// <summary>
    ///     Adds a new rule set asynchronously to the repository.
    /// </summary>
    /// <param name="ruleSet">The rule set to add.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    Task AddAsync(MonitoringRuleSet ruleSet, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the rule set of a ranch asynchronously.
    /// </summary>
    /// <param name="ranchId">The ranch that owns the rules.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the rule set if found; otherwise, <see langword="null"/>.</returns>
    Task<MonitoringRuleSet?> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads the revision, thresholds and geofences of a ranch from one consistent database snapshot asynchronously.
    /// </summary>
    /// <param name="ranchId">The ranch that owns the rules.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the snapshot; its revision is zero when the ranch never configured rules.</returns>
    Task<MonitoringRuleSnapshot> ReadSnapshotAsync(Guid ranchId, CancellationToken cancellationToken = default);
}
