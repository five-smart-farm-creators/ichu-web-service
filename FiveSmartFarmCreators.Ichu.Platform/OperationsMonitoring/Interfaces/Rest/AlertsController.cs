using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;

[ApiController]
[Route("api/v1/alerts")]
[Authorize]
[Produces("application/json")]
[TypeFilter(typeof(OperationsMonitoringExceptionFilter))]
public sealed class AlertsController(AlertService alerts) : ControllerBase
{
    /// <summary>Lists the alerts of an authorized ranch, newest event first. Status is Pending or Acknowledged.</summary>
    [HttpGet]
    public async Task<ActionResult<AlertPageResource>> GetAlerts(
        [FromQuery] Guid ranchId,
        [FromQuery] string? status,
        [FromQuery] Guid? animalId,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var query = new GetAlertsQuery(ranchId, MonitoringValueParser.ParseOptionalEnum<AlertStatus>(status, "status"), animalId, page, pageSize);
        return Ok(AlertPageResource.From(await alerts.GetAlertsAsync(query, cancellationToken)));
    }

    /// <summary>
    /// Acknowledges a pending alert. Repeating the same operation returns the stored result; a stale version or a
    /// different operation returns 409 with the current alert.
    /// </summary>
    [HttpPatch("{alertId:guid}")]
    [ProducesResponseType<AlertResource>(StatusCodes.Status200OK)]
    [ProducesResponseType<AlertResource>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AlertResource>> Acknowledge(Guid alertId, AcknowledgeAlertResource resource, CancellationToken cancellationToken)
    {
        var result = await alerts.AcknowledgeAsync(resource.ToCommand(alertId), cancellationToken);
        var body = AlertResource.From(result.Alert);
        return result.Outcome == AlertChangeOutcome.Conflict ? Conflict(body) : Ok(body);
    }
}
