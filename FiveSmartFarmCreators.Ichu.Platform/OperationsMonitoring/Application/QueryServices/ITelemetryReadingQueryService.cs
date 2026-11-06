using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;

/// <summary>
///     Application query service interface for handling telemetry reading read operations.
/// </summary>
public interface ITelemetryReadingQueryService
{
    /// <summary>
    ///     Handles the listing of the readings of an authorized animal asynchronously.
    /// </summary>
    /// <param name="query">The query containing the period and the page.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the history on success, or an error enum on failure.</returns>
    Task<Result<TelemetryHistory, EGetAnimalTelemetryError>> Handle(GetAnimalTelemetryQuery query, CancellationToken cancellationToken);

    /// <summary>
    ///     Handles the retrieval of the latest valid indicators of an authorized animal asynchronously.
    /// </summary>
    /// <param name="query">The query containing the animal identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the latest indicators on success, or an error enum on failure.</returns>
    Task<Result<LatestTelemetry, EGetLatestAnimalTelemetryError>> Handle(GetLatestAnimalTelemetryQuery query, CancellationToken cancellationToken);
}
