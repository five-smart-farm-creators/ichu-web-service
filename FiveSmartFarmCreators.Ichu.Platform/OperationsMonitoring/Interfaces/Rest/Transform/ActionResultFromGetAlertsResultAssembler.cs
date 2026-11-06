using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles an HTTP action result from the result of listing the alerts of a ranch.
/// </summary>
public static class ActionResultFromGetAlertsResultAssembler
{
    /// <summary>
    ///     Assembles an <see cref="ActionResult"/> from the result of listing the alerts of a ranch.
    /// </summary>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static ActionResult ToActionResultFromGetAlertsResult(
        Result<PagedResult<MonitoringAlert>, EGetAlertsError> result,
        ControllerBase controller) =>
        result switch
        {
            Result<PagedResult<MonitoringAlert>, EGetAlertsError>.Success success =>
                controller.Ok(AlertPageResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),
            Result<PagedResult<MonitoringAlert>, EGetAlertsError>.Failure failure => failure.Error switch
            {
                EGetAlertsError.Unauthenticated =>
                    controller.Problem(title: "A verified account is required.", statusCode: StatusCodes.Status401Unauthorized),
                EGetAlertsError.Forbidden =>
                    controller.Problem(title: "The account is not authorized for this ranch operation.", statusCode: StatusCodes.Status403Forbidden),
                EGetAlertsError.InvalidPage =>
                    controller.Problem(title: "The page must be positive and the page size within the allowed range.", statusCode: StatusCodes.Status400BadRequest),
                _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
            },
            _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
}
