using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;

/// <summary>
///     Lists the alerts of ranches and records the attention that authorized users give them.
/// </summary>
[ApiController]
[Route("api/v1/alerts")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Alerts")]
public class AlertsController(
    IMonitoringAlertCommandService alertCommandService,
    IMonitoringAlertQueryService alertQueryService,
    IStringLocalizer<OperationsMonitoringMessages> localizer) : ControllerBase
{
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
        var query = GetAlertsQueryFromRequestAssembler.ToQueryFromRequest(ranchId, status, animalId, page, pageSize);
        if (query.IsFailure)
        {
            return OperationsMonitoringActionResultAssembler.ToProblem(this, query.DomainError, localizer);
        }

        var result = await alertQueryService.Handle(query.Value!, cancellationToken);
        return OperationsMonitoringActionResultAssembler.ToActionResult(this, result, localizer,
            alertPage => Ok(AlertPageResourceFromEntityAssembler.ToResourceFromEntity(alertPage)));
    }

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
        var command = AcknowledgeAlertCommandFromResourceAssembler.ToCommandFromResource(alertId, resource);
        if (command.IsFailure)
        {
            return OperationsMonitoringActionResultAssembler.ToProblem(this, command.DomainError, localizer);
        }

        var result = await alertCommandService.Handle(command.Value!, cancellationToken);
        var currentAlert = result.IsFailure && result.DomainError.Code == OperationsMonitoringErrors.AlertConflict.Code
            ? await FindStoredAlertAsync(alertId, cancellationToken)
            : null;
        return OperationsMonitoringActionResultAssembler.ToAcknowledgeAlertActionResult(this, result, currentAlert, localizer,
            acknowledgedAlert => Ok(AlertResourceFromEntityAssembler.ToResourceFromEntity(acknowledgedAlert)));
    }

    private async Task<MonitoringAlert?> FindStoredAlertAsync(Guid alertId, CancellationToken cancellationToken)
    {
        var found = await alertQueryService.Handle(new GetAlertByIdQuery(new MonitoringAlertId(alertId)), cancellationToken);
        return found.IsSuccess ? found.Value : null;
    }
}
