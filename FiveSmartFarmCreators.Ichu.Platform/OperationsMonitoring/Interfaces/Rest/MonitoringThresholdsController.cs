using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Services;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;

/// <summary>
///     Monitoring thresholds' controller.
/// </summary>
/// <param name="thresholdCommandService">The monitoring threshold command service.</param>
/// <param name="thresholdQueryService">The monitoring threshold query service.</param>
[ApiController]
[Route("api/v1/monitoring-thresholds")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Monitoring Thresholds")]
public class MonitoringThresholdsController(
    IMonitoringThresholdCommandService thresholdCommandService,
    IMonitoringThresholdQueryService thresholdQueryService) : ControllerBase
{
    /// <summary>
    ///     Creates or replaces the threshold of a ranch for one life stage and metric, advancing the rule revision.
    /// </summary>
    /// <param name="resource">The threshold definition.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A response as an action result containing the stored threshold, or the failure of the request.</returns>
    [HttpPut]
    [ProducesResponseType(typeof(ThresholdResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Configure(ConfigureThresholdResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var command = ConfigureThresholdCommandFromResourceAssembler.ToCommandFromResource(resource);
            var result = await thresholdCommandService.Handle(command, cancellationToken);
            return ActionResultFromConfigureThresholdResultAssembler.ToActionResultFromConfigureThresholdResult(result, this);
        }
        catch (ArgumentException ex)
        {
            return Problem(title: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    /// <summary>
    ///     Lists the thresholds configured by an authorized ranch.
    /// </summary>
    /// <param name="ranchId">The ranch that configured the thresholds.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A response as an action result containing the thresholds, or the failure of the request.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ThresholdResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> GetByRanch([FromQuery] Guid ranchId, CancellationToken cancellationToken)
    {
        var result = await thresholdQueryService.Handle(new GetThresholdsByRanchQuery(ranchId), cancellationToken);
        return ActionResultFromGetThresholdsResultAssembler.ToActionResultFromGetThresholdsResult(result, this);
    }
}
