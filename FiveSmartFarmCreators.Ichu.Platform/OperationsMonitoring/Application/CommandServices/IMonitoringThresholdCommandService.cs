using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;

public interface IMonitoringThresholdCommandService
{
    /// <summary>
    ///     Creates or replaces the threshold of a ranch and advances the rule revision.
    /// </summary>
    Task<Result<MonitoringThreshold>> Handle(ConfigureThresholdCommand command, CancellationToken cancellationToken);
}
