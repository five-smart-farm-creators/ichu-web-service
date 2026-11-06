using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles an HTTP action result from the result of registering a livestock operation.
/// </summary>
public static class ActionResultFromRegisterLivestockOperationResultAssembler
{
    /// <summary>
    ///     Assembles an <see cref="ActionResult"/> from the result of registering a livestock operation. A retry of a
    ///     registered operation returns the stored record with 200.
    /// </summary>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static ActionResult ToActionResultFromRegisterLivestockOperationResult(
        Result<LivestockOperationRegistration, ERegisterLivestockOperationError> result,
        ControllerBase controller) =>
        result switch
        {
            Result<LivestockOperationRegistration, ERegisterLivestockOperationError>.Success { Value.Created: true } success =>
                controller.Created($"/api/v1/animals/{success.Value.Operation.CattleId}/livestock-operations",
                    LivestockOperationResourceFromEntityAssembler.ToResourceFromEntity(success.Value.Operation)),
            Result<LivestockOperationRegistration, ERegisterLivestockOperationError>.Success success =>
                controller.Ok(LivestockOperationResourceFromEntityAssembler.ToResourceFromEntity(success.Value.Operation)),
            Result<LivestockOperationRegistration, ERegisterLivestockOperationError>.Failure failure => failure.Error switch
            {
                ERegisterLivestockOperationError.Unauthenticated =>
                    controller.Problem(title: "A verified account is required.", statusCode: StatusCodes.Status401Unauthorized),
                ERegisterLivestockOperationError.AnimalNotFound =>
                    controller.Problem(title: "The animal does not exist.", statusCode: StatusCodes.Status404NotFound),
                ERegisterLivestockOperationError.Forbidden =>
                    controller.Problem(title: "The account is not authorized for this ranch operation.", statusCode: StatusCodes.Status403Forbidden),
                ERegisterLivestockOperationError.EventTimeAhead =>
                    controller.Problem(title: "The event time is ahead of the server clock beyond the configured tolerance.", statusCode: StatusCodes.Status400BadRequest),
                ERegisterLivestockOperationError.AnimalNotActive =>
                    controller.Problem(title: "The animal was not active in the herd when the operation occurred.", statusCode: StatusCodes.Status409Conflict),
                ERegisterLivestockOperationError.OperationConflict =>
                    controller.Problem(title: "A different operation already uses this identifier.", statusCode: StatusCodes.Status409Conflict),
                _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
            },
            _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
}
