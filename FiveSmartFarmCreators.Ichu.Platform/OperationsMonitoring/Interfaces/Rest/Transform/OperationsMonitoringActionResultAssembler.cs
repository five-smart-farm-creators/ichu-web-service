using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles the HTTP action results of the Operations and Monitoring endpoints from application results. A
///     failure becomes a problem response whose title is the message of the domain error.
/// </summary>
public static class OperationsMonitoringActionResultAssembler
{
    private static int ToStatusCodeFromOperationsMonitoringError(OperationsMonitoringError error)
    {
        return error switch
        {
            OperationsMonitoringError.Unauthenticated => StatusCodes.Status401Unauthorized,
            OperationsMonitoringError.GatewayUnauthenticated => StatusCodes.Status401Unauthorized,
            OperationsMonitoringError.Forbidden => StatusCodes.Status403Forbidden,
            OperationsMonitoringError.GatewayMismatch => StatusCodes.Status403Forbidden,
            OperationsMonitoringError.AlertNotFound => StatusCodes.Status404NotFound,
            OperationsMonitoringError.AnimalNotFound => StatusCodes.Status404NotFound,
            OperationsMonitoringError.LotNotFound => StatusCodes.Status404NotFound,
            OperationsMonitoringError.GeofenceNotFound => StatusCodes.Status404NotFound,
            OperationsMonitoringError.GeofenceAlreadyDefined => StatusCodes.Status409Conflict,
            OperationsMonitoringError.AlertConflict => StatusCodes.Status409Conflict,
            OperationsMonitoringError.OperationConflict => StatusCodes.Status409Conflict,
            OperationsMonitoringError.AnimalNotActive => StatusCodes.Status409Conflict,
            OperationsMonitoringError.ConcurrentModification => StatusCodes.Status409Conflict,
            OperationsMonitoringError.EventTimeAhead => StatusCodes.Status400BadRequest,
            OperationsMonitoringError.InvalidPage => StatusCodes.Status400BadRequest,
            OperationsMonitoringError.InvalidRevision => StatusCodes.Status400BadRequest,
            OperationsMonitoringError.InvalidEdgeBatchSize => StatusCodes.Status400BadRequest,
            OperationsMonitoringError.InvalidMobileBatchSize => StatusCodes.Status400BadRequest,
            OperationsMonitoringError.RulesNotModified => StatusCodes.Status304NotModified,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static IActionResult ToProblemFromFailure<T>(ControllerBase controller, Result<T> result)
    {
        var statusCode = ToStatusCodeFromOperationsMonitoringError((OperationsMonitoringError)result.Error!);
        return controller.Problem(title: result.Message, statusCode: statusCode);
    }

    /// <summary>
    ///     Assembles the action result of synchronizing an Edge Gateway batch.
    /// </summary>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="successAction">The function that produces the response of a success.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static IActionResult ToActionResultFromSynchronizeEdgeBatchResult(
        ControllerBase controller,
        Result<IReadOnlyList<SyncEntryResult>> result,
        Func<IReadOnlyList<SyncEntryResult>, IActionResult> successAction)
    {
        return result.IsSuccess ? successAction(result.Value!) : ToProblemFromFailure(controller, result);
    }

    /// <summary>
    ///     Assembles the action result of listing the readings of an animal.
    /// </summary>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="successAction">The function that produces the response of a success.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static IActionResult ToActionResultFromGetAnimalTelemetryResult(
        ControllerBase controller,
        Result<TelemetryHistory> result,
        Func<TelemetryHistory, IActionResult> successAction)
    {
        return result.IsSuccess ? successAction(result.Value!) : ToProblemFromFailure(controller, result);
    }

    /// <summary>
    ///     Assembles the action result of retrieving the latest indicators of an animal.
    /// </summary>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="successAction">The function that produces the response of a success.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static IActionResult ToActionResultFromGetLatestAnimalTelemetryResult(
        ControllerBase controller,
        Result<LatestTelemetry> result,
        Func<LatestTelemetry, IActionResult> successAction)
    {
        return result.IsSuccess ? successAction(result.Value!) : ToProblemFromFailure(controller, result);
    }

    /// <summary>
    ///     Assembles the action result of listing the alerts of a ranch.
    /// </summary>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="successAction">The function that produces the response of a success.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static IActionResult ToActionResultFromGetAlertsResult(
        ControllerBase controller,
        Result<PagedResult<MonitoringAlert>> result,
        Func<PagedResult<MonitoringAlert>, IActionResult> successAction)
    {
        return result.IsSuccess ? successAction(result.Value!) : ToProblemFromFailure(controller, result);
    }

    /// <summary>
    ///     Assembles the action result of acknowledging an alert. A conflict returns the stored alert when it is
    ///     available, so the client can reconcile its copy.
    /// </summary>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="currentAlert">The stored alert, returned with a conflict; it is only needed on failure.</param>
    /// <param name="successAction">The function that produces the response of a success.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static IActionResult ToActionResultFromAcknowledgeAlertResult(
        ControllerBase controller,
        Result<MonitoringAlert> result,
        MonitoringAlert? currentAlert,
        Func<MonitoringAlert, IActionResult> successAction)
    {
        if (result.IsSuccess)
        {
            return successAction(result.Value!);
        }

        if (result.Error is OperationsMonitoringError.AlertConflict && currentAlert is not null)
        {
            return controller.Conflict(AlertResourceFromEntityAssembler.ToResourceFromEntity(currentAlert));
        }

        return ToProblemFromFailure(controller, result);
    }

    /// <summary>
    ///     Assembles the action result of configuring a threshold.
    /// </summary>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="successAction">The function that produces the response of a success.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static IActionResult ToActionResultFromConfigureThresholdResult(
        ControllerBase controller,
        Result<MonitoringThreshold> result,
        Func<MonitoringThreshold, IActionResult> successAction)
    {
        return result.IsSuccess ? successAction(result.Value!) : ToProblemFromFailure(controller, result);
    }

    /// <summary>
    ///     Assembles the action result of listing the thresholds of a ranch.
    /// </summary>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="successAction">The function that produces the response of a success.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static IActionResult ToActionResultFromGetThresholdsResult(
        ControllerBase controller,
        Result<IReadOnlyList<MonitoringThreshold>> result,
        Func<IReadOnlyList<MonitoringThreshold>, IActionResult> successAction)
    {
        return result.IsSuccess ? successAction(result.Value!) : ToProblemFromFailure(controller, result);
    }

    /// <summary>
    ///     Assembles the action result of defining a geofence.
    /// </summary>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="successAction">The function that produces the response of a success.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static IActionResult ToActionResultFromDefineGeofenceResult(
        ControllerBase controller,
        Result<Geofence> result,
        Func<Geofence, IActionResult> successAction)
    {
        return result.IsSuccess ? successAction(result.Value!) : ToProblemFromFailure(controller, result);
    }

    /// <summary>
    ///     Assembles the action result of redefining a geofence.
    /// </summary>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="successAction">The function that produces the response of a success.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static IActionResult ToActionResultFromRedefineGeofenceResult(
        ControllerBase controller,
        Result<Geofence> result,
        Func<Geofence, IActionResult> successAction)
    {
        return result.IsSuccess ? successAction(result.Value!) : ToProblemFromFailure(controller, result);
    }

    /// <summary>
    ///     Assembles the action result of listing the geofences of a ranch.
    /// </summary>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="successAction">The function that produces the response of a success.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static IActionResult ToActionResultFromGetGeofencesResult(
        ControllerBase controller,
        Result<IReadOnlyList<Geofence>> result,
        Func<IReadOnlyList<Geofence>, IActionResult> successAction)
    {
        return result.IsSuccess ? successAction(result.Value!) : ToProblemFromFailure(controller, result);
    }

    /// <summary>
    ///     Assembles the action result of reading the rules that an Edge Gateway does not hold yet. A gateway that
    ///     already holds the current revision receives 304 without a body.
    /// </summary>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="successAction">The function that produces the response of a success.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static IActionResult ToActionResultFromGetEdgeMonitoringRulesResult(
        ControllerBase controller,
        Result<MonitoringRuleSnapshot> result,
        Func<MonitoringRuleSnapshot, IActionResult> successAction)
    {
        if (result.IsSuccess)
        {
            return successAction(result.Value!);
        }

        // A 304 response cannot carry a problem body.
        return result.Error is OperationsMonitoringError.RulesNotModified
            ? controller.StatusCode(StatusCodes.Status304NotModified)
            : ToProblemFromFailure(controller, result);
    }

    /// <summary>
    ///     Assembles the action result of registering a livestock operation.
    /// </summary>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="successAction">
    ///     The function that produces the response of a success; it distinguishes a new registration from a retry.
    /// </param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static IActionResult ToActionResultFromRegisterLivestockOperationResult(
        ControllerBase controller,
        Result<LivestockOperationRegistration> result,
        Func<LivestockOperationRegistration, IActionResult> successAction)
    {
        return result.IsSuccess ? successAction(result.Value!) : ToProblemFromFailure(controller, result);
    }

    /// <summary>
    ///     Assembles the action result of synchronizing a mobile batch.
    /// </summary>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="successAction">The function that produces the response of a success.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static IActionResult ToActionResultFromSynchronizeMobileBatchResult(
        ControllerBase controller,
        Result<IReadOnlyList<SyncEntryResult>> result,
        Func<IReadOnlyList<SyncEntryResult>, IActionResult> successAction)
    {
        return result.IsSuccess ? successAction(result.Value!) : ToProblemFromFailure(controller, result);
    }

    /// <summary>
    ///     Assembles the action result of listing the livestock operations of an animal.
    /// </summary>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="successAction">The function that produces the response of a success.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static IActionResult ToActionResultFromGetLivestockOperationsResult(
        ControllerBase controller,
        Result<IReadOnlyList<LivestockOperation>> result,
        Func<IReadOnlyList<LivestockOperation>, IActionResult> successAction)
    {
        return result.IsSuccess ? successAction(result.Value!) : ToProblemFromFailure(controller, result);
    }
}
