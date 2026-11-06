using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles an HTTP action result from the result of reading the rules that an Edge Gateway does not hold yet.
/// </summary>
public static class ActionResultFromGetEdgeMonitoringRulesResultAssembler
{
    /// <summary>
    ///     Assembles an <see cref="ActionResult"/> from the result of reading the rules of a gateway.
    /// </summary>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static ActionResult ToActionResultFromGetEdgeMonitoringRulesResult(
        Result<MonitoringRuleSnapshot, EGetEdgeMonitoringRulesError> result,
        ControllerBase controller) =>
        result switch
        {
            Result<MonitoringRuleSnapshot, EGetEdgeMonitoringRulesError>.Success success =>
                controller.Ok(MonitoringRulesResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),
            Result<MonitoringRuleSnapshot, EGetEdgeMonitoringRulesError>.Failure failure => failure.Error switch
            {
                EGetEdgeMonitoringRulesError.NotModified =>
                    controller.StatusCode(StatusCodes.Status304NotModified),
                EGetEdgeMonitoringRulesError.Unauthenticated =>
                    controller.Problem(title: "Valid Edge Gateway credentials are required.", statusCode: StatusCodes.Status401Unauthorized),
                EGetEdgeMonitoringRulesError.GatewayMismatch =>
                    controller.Problem(title: "A gateway can only read its own configuration.", statusCode: StatusCodes.Status403Forbidden),
                EGetEdgeMonitoringRulesError.InvalidRevision =>
                    controller.Problem(title: "The known revision cannot be negative.", statusCode: StatusCodes.Status400BadRequest),
                _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
            },
            _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
}
