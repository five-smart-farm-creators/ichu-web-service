using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles an HTTP action result from the result of synchronizing an Edge Gateway batch.
/// </summary>
public static class ActionResultFromSynchronizeEdgeBatchResultAssembler
{
    /// <summary>
    ///     Assembles an <see cref="ActionResult"/> from the result of synchronizing an Edge Gateway batch.
    /// </summary>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static ActionResult ToActionResultFromSynchronizeEdgeBatchResult(
        Result<IReadOnlyList<SyncEntryResult>, ESynchronizeEdgeBatchError> result,
        ControllerBase controller) =>
        result switch
        {
            Result<IReadOnlyList<SyncEntryResult>, ESynchronizeEdgeBatchError>.Success success =>
                controller.Ok(SyncBatchResultResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),
            Result<IReadOnlyList<SyncEntryResult>, ESynchronizeEdgeBatchError>.Failure failure => failure.Error switch
            {
                ESynchronizeEdgeBatchError.Unauthenticated =>
                    controller.Problem(title: "Valid Edge Gateway credentials are required.", statusCode: StatusCodes.Status401Unauthorized),
                ESynchronizeEdgeBatchError.InvalidBatchSize =>
                    controller.Problem(title: "A batch must contain at least one entry and no more than the configured maximum.", statusCode: StatusCodes.Status400BadRequest),
                _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
            },
            _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
}
