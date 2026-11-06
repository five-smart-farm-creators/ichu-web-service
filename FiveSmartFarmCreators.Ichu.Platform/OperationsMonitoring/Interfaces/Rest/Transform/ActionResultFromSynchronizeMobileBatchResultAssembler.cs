using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles an HTTP action result from the result of synchronizing a mobile batch.
/// </summary>
public static class ActionResultFromSynchronizeMobileBatchResultAssembler
{
    /// <summary>
    ///     Assembles an <see cref="ActionResult"/> from the result of synchronizing a mobile batch.
    /// </summary>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static ActionResult ToActionResultFromSynchronizeMobileBatchResult(
        Result<IReadOnlyList<SyncEntryResult>, ESynchronizeMobileBatchError> result,
        ControllerBase controller) =>
        result switch
        {
            Result<IReadOnlyList<SyncEntryResult>, ESynchronizeMobileBatchError>.Success success =>
                controller.Ok(SyncBatchResultResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),
            Result<IReadOnlyList<SyncEntryResult>, ESynchronizeMobileBatchError>.Failure failure => failure.Error switch
            {
                ESynchronizeMobileBatchError.Unauthenticated =>
                    controller.Problem(title: "A verified account is required.", statusCode: StatusCodes.Status401Unauthorized),
                ESynchronizeMobileBatchError.InvalidBatchSize =>
                    controller.Problem(title: "A batch must contain at least one operation and no more than the configured maximum.", statusCode: StatusCodes.Status400BadRequest),
                _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
            },
            _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
}
