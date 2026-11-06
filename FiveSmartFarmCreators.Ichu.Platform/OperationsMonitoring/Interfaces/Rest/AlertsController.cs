using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;

/// <summary>
///     Alerts' controller.
/// </summary>
/// <param name="alertCommandService">The monitoring alert command service.</param>
/// <param name="alertQueryService">The monitoring alert query service.</param>
/// <param name="logger">Logger for diagnostics and error reporting.</param>
[ApiController]
[Route("api/v1/alerts")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Alerts")]
public class AlertsController(
    IMonitoringAlertCommandService alertCommandService,
    IMonitoringAlertQueryService alertQueryService,
    ILogger<AlertsController> logger) : ControllerBase
{
    /// <summary>
    ///     Lists the alerts of an authorized ranch, newest event first. Status is Pending or Acknowledged.
    /// </summary>
    /// <param name="ranchId">The ranch that owns the alerts.</param>
    /// <param name="status">Only alerts with this status, when given.</param>
    /// <param name="animalId">Only alerts of this animal, when given.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of alerts per page.</param>
    /// <returns>A response as an action result containing the page of alerts, or the failure of the request.</returns>
    [HttpGet]
    [EndpointSummary("List the alerts of a ranch")]
    [EndpointDescription("Lists the alerts of an authorized ranch, newest event first, optionally filtered by status (Pending or Acknowledged) and animal.")]
    [ProducesResponseType(typeof(AlertPageResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] Guid ranchId,
        [FromQuery] string? status,
        [FromQuery] Guid? animalId,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        try
        {
            var query = GetAlertsQueryFromRequestAssembler.ToQueryFromRequest(ranchId, status, animalId, page, pageSize);
            var result = await alertQueryService.Handle(query, cancellationToken);
            return OperationsMonitoringActionResultAssembler.ToActionResultFromGetAlertsResult(
                this,
                result,
                alertPage => Ok(AlertPageResourceFromEntityAssembler.ToResourceFromEntity(alertPage)));
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Rejected an invalid request: {Reason}", ex.Message);
            return Problem(title: "The request is not valid.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    /// <summary>
    ///     Acknowledges a pending alert. Repeating the same operation returns the stored result; a stale version or a
    ///     different operation returns 409 with the current alert.
    /// </summary>
    /// <param name="alertId">The alert to attend.</param>
    /// <param name="resource">The attention data.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A response as an action result containing the stored alert, or the failure of the request.</returns>
    [HttpPatch("{alertId:guid}")]
    [EndpointSummary("Acknowledge an alert")]
    [EndpointDescription("Acknowledges a pending alert. Repeating the same operation returns the stored result; a stale version or a different operation returns 409 with the current alert.")]
    [ProducesResponseType(typeof(AlertResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AlertResource), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Acknowledge(Guid alertId, AcknowledgeAlertResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var command = AcknowledgeAlertCommandFromResourceAssembler.ToCommandFromResource(alertId, resource);
            var result = await alertCommandService.Handle(command, cancellationToken);
            var currentAlert = result.Error is OperationsMonitoringError.AlertConflict
                ? await FindStoredAlertAsync(alertId, cancellationToken)
                : null;
            return OperationsMonitoringActionResultAssembler.ToActionResultFromAcknowledgeAlertResult(
                this,
                result,
                currentAlert,
                acknowledgedAlert => Ok(AlertResourceFromEntityAssembler.ToResourceFromEntity(acknowledgedAlert)));
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Rejected an invalid request: {Reason}", ex.Message);
            return Problem(title: "The request is not valid.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    private async Task<MonitoringAlert?> FindStoredAlertAsync(Guid alertId, CancellationToken cancellationToken)
    {
        var found = await alertQueryService.Handle(new GetAlertByIdQuery(alertId), cancellationToken);
        return found.IsSuccess ? found.Value : null;
    }
}
