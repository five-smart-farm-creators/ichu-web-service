using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Maps application results to HTTP responses. A business error becomes a problem response whose title is the localized message and whose code lets clients branch on the failure.
/// </summary>
public static class OperationsMonitoringActionResultAssembler
{
    public static IActionResult ToActionResult<T>(
        ControllerBase controller,
        Result<T> result,
        IStringLocalizer<OperationsMonitoringMessages> localizer,
        Func<T, IActionResult> successAction) =>
        result.IsSuccess ? successAction(result.Value!) : ToProblem(controller, result.DomainError, localizer);

    public static IActionResult ToProblem(ControllerBase controller, Error error, IStringLocalizer<OperationsMonitoringMessages> localizer)
    {
        var response = controller.Problem(title: localizer[error.Code].Value, statusCode: ToStatusCode(error.Type));
        if (response.Value is ProblemDetails problem)
        {
            problem.Extensions["code"] = error.Code;
        }

        return response;
    }

    public static IActionResult ToAcknowledgeAlertActionResult(
        ControllerBase controller,
        Result<MonitoringAlert> result,
        MonitoringAlert? currentAlert,
        IStringLocalizer<OperationsMonitoringMessages> localizer,
        Func<MonitoringAlert, IActionResult> successAction)
    {
        if (result.IsSuccess)
        {
            return successAction(result.Value!);
        }

        // A conflict returns the stored alert when it is available, so the client can reconcile its copy.
        return result.DomainError.Code == OperationsMonitoringErrors.AlertConflict.Code && currentAlert is not null
            ? controller.Conflict(AlertResourceFromEntityAssembler.ToResourceFromEntity(currentAlert))
            : ToProblem(controller, result.DomainError, localizer);
    }

    public static IActionResult ToMonitoringRulesActionResult(
        ControllerBase controller,
        Result<MonitoringRuleSnapshot> result,
        IStringLocalizer<OperationsMonitoringMessages> localizer,
        Func<MonitoringRuleSnapshot, IActionResult> successAction)
    {
        if (result.IsSuccess)
        {
            return successAction(result.Value!);
        }

        // A 304 response cannot carry a problem body.
        return result.DomainError.Code == OperationsMonitoringErrors.RulesNotModified.Code
            ? controller.StatusCode(StatusCodes.Status304NotModified)
            : ToProblem(controller, result.DomainError, localizer);
    }

    private static int ToStatusCode(EErrorType type) => type switch
    {
        EErrorType.Validation => StatusCodes.Status400BadRequest,
        EErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        EErrorType.Forbidden => StatusCodes.Status403Forbidden,
        EErrorType.NotFound => StatusCodes.Status404NotFound,
        EErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };
}
