using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

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
    /// <returns>A result containing the operations on success, or the domain error on failure.</returns>
    Task<Result<IReadOnlyList<LivestockOperation>>> Handle(GetLivestockOperationsQuery query, CancellationToken cancellationToken);
}
