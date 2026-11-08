using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;

/// <summary>
///     Publishes the monitoring rules to Edge Gateways. Gateway credentials are verified by the gateway authenticator, not by user authentication.
/// </summary>
[ApiController]
[Route("api/v1/edge-gateways")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Edge Gateways")]
public class EdgeGatewaysController(
    IMonitoringRuleSetQueryService ruleSetQueryService,
    IStringLocalizer<OperationsMonitoringMessages> localizer) : ControllerBase
{
    [HttpGet("{gatewayId:guid}/monitoring-rules")]
    [EndpointSummary("Get the monitoring rules of an Edge Gateway")]
    [EndpointDescription("Returns the thresholds and geofences of the gateway's ranch when its revision is newer than afterRevision, or 304 when the gateway already holds the current revision. Requires Edge Gateway credentials.")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MonitoringRulesResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMonitoringRules(
        Guid gatewayId,
        CancellationToken cancellationToken,
        [FromQuery] long afterRevision = 0)
    {
        var result = await ruleSetQueryService.Handle(new GetEdgeMonitoringRulesQuery(gatewayId, afterRevision), cancellationToken);
        return OperationsMonitoringActionResultAssembler.ToMonitoringRulesActionResult(this, result, localizer,
            snapshot => Ok(MonitoringRulesResourceFromEntityAssembler.ToResourceFromEntity(snapshot)));
    }
}
