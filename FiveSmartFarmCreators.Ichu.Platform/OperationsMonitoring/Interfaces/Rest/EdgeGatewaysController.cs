using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;

[ApiController]
[Route("api/v1/edge-gateways")]
[Produces("application/json")]
[TypeFilter(typeof(OperationsMonitoringExceptionFilter))]
public sealed class EdgeGatewaysController(MonitoringRulesService rules) : ControllerBase
{
    /// <summary>
    /// Returns the thresholds and geofences of the gateway's ranch when its revision is newer than afterRevision,
    /// or 304 when the gateway already holds the current revision.
    /// </summary>
    /// <remarks>Gateway credentials are verified by the Edge Gateway authenticator, not by user authentication.</remarks>
    [HttpGet("{gatewayId:guid}/monitoring-rules")]
    [AllowAnonymous]
    [ProducesResponseType<MonitoringRulesResource>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public async Task<ActionResult<MonitoringRulesResource>> GetMonitoringRules(
        Guid gatewayId,
        CancellationToken cancellationToken,
        [FromQuery] long afterRevision = 0)
    {
        var snapshot = await rules.GetEdgeRulesAsync(gatewayId, afterRevision, cancellationToken);
        return snapshot is null ? StatusCode(StatusCodes.Status304NotModified) : Ok(MonitoringRulesResource.From(snapshot));
    }
}
