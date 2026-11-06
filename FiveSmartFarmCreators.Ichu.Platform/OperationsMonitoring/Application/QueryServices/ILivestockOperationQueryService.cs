using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;

/// <summary>
///     Application query service interface for handling livestock operation read operations.
/// </summary>
public interface ILivestockOperationQueryService
{
    /// <summary>
    ///     Handles the listing of the livestock operations of an authorized animal asynchronously.
    /// </summary>
    /// <param name="query">The query containing the animal and the period.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the operations on success, or an error enum on failure.</returns>
    Task<Result<IReadOnlyList<LivestockOperation>, EGetLivestockOperationsError>> Handle(GetLivestockOperationsQuery query, CancellationToken cancellationToken);
}
