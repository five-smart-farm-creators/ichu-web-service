using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles an HTTP action result from the result of acknowledging an alert.
/// </summary>
public static class ActionResultFromAcknowledgeAlertResultAssembler
{
    /// <summary>
    ///     Assembles an <see cref="ActionResult"/> from the result of acknowledging an alert.
    /// </summary>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <param name="currentAlert">The stored alert, returned with a conflict; it is only needed on failure.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static ActionResult ToActionResultFromAcknowledgeAlertResult(
        Result<MonitoringAlert, EAcknowledgeAlertError> result,
        ControllerBase controller,
        MonitoringAlert? currentAlert) =>
        result switch
        {
            Result<MonitoringAlert, EAcknowledgeAlertError>.Success success =>
                controller.Ok(AlertResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),
            Result<MonitoringAlert, EAcknowledgeAlertError>.Failure failure => failure.Error switch
            {
                EAcknowledgeAlertError.Unauthenticated =>
                    controller.Problem(title: "A verified account is required.", statusCode: StatusCodes.Status401Unauthorized),
                EAcknowledgeAlertError.AlertNotFound =>
                    controller.Problem(title: "The alert does not exist.", statusCode: StatusCodes.Status404NotFound),
                EAcknowledgeAlertError.Forbidden =>
                    controller.Problem(title: "The account is not authorized for this ranch operation.", statusCode: StatusCodes.Status403Forbidden),
                EAcknowledgeAlertError.Conflict when currentAlert is not null =>
                    controller.Conflict(AlertResourceFromEntityAssembler.ToResourceFromEntity(currentAlert)),
                EAcknowledgeAlertError.Conflict =>
                    controller.Problem(title: "The alert was already attended or changed by a different operation.", statusCode: StatusCodes.Status409Conflict),
                _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
            },
            _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
}
