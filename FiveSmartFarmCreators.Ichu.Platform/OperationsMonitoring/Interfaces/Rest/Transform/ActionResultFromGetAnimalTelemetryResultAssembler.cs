using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles an HTTP action result from the result of listing the readings of an animal.
/// </summary>
public static class ActionResultFromGetAnimalTelemetryResultAssembler
{
    /// <summary>
    ///     Assembles an <see cref="ActionResult"/> from the result of listing the readings of an animal.
    /// </summary>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static ActionResult ToActionResultFromGetAnimalTelemetryResult(
        Result<TelemetryHistory, EGetAnimalTelemetryError> result,
        ControllerBase controller) =>
        result switch
        {
            Result<TelemetryHistory, EGetAnimalTelemetryError>.Success success =>
                controller.Ok(TelemetryHistoryResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),
            Result<TelemetryHistory, EGetAnimalTelemetryError>.Failure failure => failure.Error switch
            {
                EGetAnimalTelemetryError.Unauthenticated =>
                    controller.Problem(title: "A verified account is required.", statusCode: StatusCodes.Status401Unauthorized),
                EGetAnimalTelemetryError.AnimalNotFound =>
                    controller.Problem(title: "The animal does not exist.", statusCode: StatusCodes.Status404NotFound),
                EGetAnimalTelemetryError.Forbidden =>
                    controller.Problem(title: "The account is not authorized for this ranch operation.", statusCode: StatusCodes.Status403Forbidden),
                EGetAnimalTelemetryError.InvalidPage =>
                    controller.Problem(title: "The page must be positive and the page size within the allowed range.", statusCode: StatusCodes.Status400BadRequest),
                _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
            },
            _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
}
