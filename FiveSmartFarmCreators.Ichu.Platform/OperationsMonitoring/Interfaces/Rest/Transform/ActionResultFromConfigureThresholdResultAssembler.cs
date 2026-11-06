using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles an HTTP action result from the result of configuring a threshold.
/// </summary>
public static class ActionResultFromConfigureThresholdResultAssembler
{
    /// <summary>
    ///     Assembles an <see cref="ActionResult"/> from the result of configuring a threshold.
    /// </summary>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static ActionResult ToActionResultFromConfigureThresholdResult(
        Result<MonitoringThreshold, EConfigureThresholdError> result,
        ControllerBase controller) =>
        result switch
        {
            Result<MonitoringThreshold, EConfigureThresholdError>.Success success =>
                controller.Ok(ThresholdResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),
            Result<MonitoringThreshold, EConfigureThresholdError>.Failure failure => failure.Error switch
            {
                EConfigureThresholdError.Unauthenticated =>
                    controller.Problem(title: "A verified account is required.", statusCode: StatusCodes.Status401Unauthorized),
                EConfigureThresholdError.Forbidden =>
                    controller.Problem(title: "The account is not authorized for this ranch operation.", statusCode: StatusCodes.Status403Forbidden),
                EConfigureThresholdError.ConcurrentModification =>
                    controller.Problem(title: "The rules changed concurrently. Reload them before retrying.", statusCode: StatusCodes.Status409Conflict),
                _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
            },
            _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
}
