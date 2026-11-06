using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;

/// <summary>
///     Application command service interface for handling monitoring threshold write operations.
/// </summary>
public interface IMonitoringThresholdCommandService
{
    /// <summary>
    ///     Handles the creation or replacement of the threshold of a ranch asynchronously, advancing the rule revision.
    /// </summary>
    /// <param name="command">The command containing the threshold definition.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the stored threshold on success, or the domain error on failure.</returns>
    /// <exception cref="ArgumentException">Thrown when the life stage is invalid.</exception>
    Task<Result<MonitoringThreshold>> Handle(ConfigureThresholdCommand command, CancellationToken cancellationToken);
}
