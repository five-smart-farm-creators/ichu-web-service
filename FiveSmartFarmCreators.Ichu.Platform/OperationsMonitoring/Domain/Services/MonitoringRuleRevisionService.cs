using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Services;

/// <summary>
///     Domain service that implements <see cref="IMonitoringRuleRevisionService"/> over the rule set repository.
/// </summary>
/// <param name="ruleSets">The rule set repository.</param>
public class MonitoringRuleRevisionService(IMonitoringRuleSetRepository ruleSets) : IMonitoringRuleRevisionService
{
    /// <inheritdoc />
    public async Task<long> AdvanceAsync(Guid ranchId, CancellationToken cancellationToken = default)
    {
        var ruleSet = await ruleSets.FindByRanchAsync(ranchId, cancellationToken);
        if (ruleSet is null)
        {
            ruleSet = new MonitoringRuleSet(ranchId);
            await ruleSets.AddAsync(ruleSet, cancellationToken);
        }

        return ruleSet.Advance();
    }
}
