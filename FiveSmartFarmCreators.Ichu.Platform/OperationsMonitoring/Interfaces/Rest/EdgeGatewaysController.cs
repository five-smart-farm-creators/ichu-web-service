using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;

/// <summary>
///     Edge Gateways' controller.
/// </summary>
/// <param name="ruleSetQueryService">The monitoring rule set query service.</param>
[ApiController]
[Route("api/v1/edge-gateways")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Edge Gateways")]
public class EdgeGatewaysController(IMonitoringRuleSetQueryService ruleSetQueryService) : ControllerBase
{
    /// <summary>
    ///     Returns the thresholds and geofences of the gateway's ranch when its revision is newer than afterRevision,
    ///     or 304 when the gateway already holds the current revision. Gateway credentials are verified by the Edge
    ///     Gateway authenticator, not by user authentication.
    /// </summary>
    /// <param name="gatewayId">The gateway that asks for its configuration.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <param name="afterRevision">The newest rule revision the gateway already holds.</param>
    /// <returns>A response as an action result containing the rules, or 304 when nothing is new.</returns>
    [HttpGet("{gatewayId:guid}/monitoring-rules")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MonitoringRulesResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> GetMonitoringRules(
        Guid gatewayId,
        CancellationToken cancellationToken,
        [FromQuery] long afterRevision = 0)
    {
        var result = await ruleSetQueryService.Handle(new GetEdgeMonitoringRulesQuery(gatewayId, afterRevision), cancellationToken);
        return ActionResultFromGetEdgeMonitoringRulesResultAssembler.ToActionResultFromGetEdgeMonitoringRulesResult(result, this);
    }
}
