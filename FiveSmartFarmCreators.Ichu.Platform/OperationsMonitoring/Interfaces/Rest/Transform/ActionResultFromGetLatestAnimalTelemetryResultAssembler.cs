using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles an HTTP action result from the result of retrieving the latest indicators of an animal.
/// </summary>
public static class ActionResultFromGetLatestAnimalTelemetryResultAssembler
{
    /// <summary>
    ///     Assembles an <see cref="ActionResult"/> from the result of retrieving the latest indicators of an animal.
    /// </summary>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static ActionResult ToActionResultFromGetLatestAnimalTelemetryResult(
        Result<LatestTelemetry, EGetLatestAnimalTelemetryError> result,
        ControllerBase controller) =>
        result switch
        {
            Result<LatestTelemetry, EGetLatestAnimalTelemetryError>.Success success =>
                controller.Ok(LatestTelemetryResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),
            Result<LatestTelemetry, EGetLatestAnimalTelemetryError>.Failure failure => failure.Error switch
            {
                EGetLatestAnimalTelemetryError.Unauthenticated =>
                    controller.Problem(title: "A verified account is required.", statusCode: StatusCodes.Status401Unauthorized),
                EGetLatestAnimalTelemetryError.AnimalNotFound =>
                    controller.Problem(title: "The animal does not exist.", statusCode: StatusCodes.Status404NotFound),
                EGetLatestAnimalTelemetryError.Forbidden =>
                    controller.Problem(title: "The account is not authorized for this ranch operation.", statusCode: StatusCodes.Status403Forbidden),
                _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
            },
            _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
}
