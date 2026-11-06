using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles an HTTP action result from the result of listing the thresholds of a ranch.
/// </summary>
public static class ActionResultFromGetThresholdsResultAssembler
{
    /// <summary>
    ///     Assembles an <see cref="ActionResult"/> from the result of listing the thresholds of a ranch.
    /// </summary>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static ActionResult ToActionResultFromGetThresholdsResult(
        Result<IReadOnlyList<MonitoringThreshold>, EGetThresholdsError> result,
        ControllerBase controller) =>
        result switch
        {
            Result<IReadOnlyList<MonitoringThreshold>, EGetThresholdsError>.Success success =>
                controller.Ok(success.Value.Select(ThresholdResourceFromEntityAssembler.ToResourceFromEntity).ToList()),
            Result<IReadOnlyList<MonitoringThreshold>, EGetThresholdsError>.Failure failure => failure.Error switch
            {
                EGetThresholdsError.Unauthenticated =>
                    controller.Problem(title: "A verified account is required.", statusCode: StatusCodes.Status401Unauthorized),
                EGetThresholdsError.Forbidden =>
                    controller.Problem(title: "The account is not authorized for this ranch operation.", statusCode: StatusCodes.Status403Forbidden),
                _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
            },
            _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
}
