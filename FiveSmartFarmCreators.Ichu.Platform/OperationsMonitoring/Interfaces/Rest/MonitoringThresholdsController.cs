using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
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
///     Configures and lists the normal ranges that ranches set for each life stage and metric.
/// </summary>
[ApiController]
[Route("api/v1/ranches/{ranchId:guid}/monitoring-thresholds")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Monitoring Thresholds")]
public class MonitoringThresholdsController(
    IMonitoringThresholdCommandService thresholdCommandService,
    IMonitoringThresholdQueryService thresholdQueryService,
    IStringLocalizer<OperationsMonitoringMessages> localizer) : ControllerBase
{
    [HttpPut]
    [EndpointSummary("Configure a threshold")]
    [EndpointDescription("Creates or replaces the threshold of a ranch for one life stage, metric and severity, advancing the rule revision. A ranch keeps at most one Warning and one Critical threshold per life stage and metric, and the Critical range must contain the Warning range; otherwise the request returns HTTP 400 with OperationsMonitoring.ThresholdLevelsInconsistent.")]
    [ProducesResponseType(typeof(ThresholdResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Configure(Guid ranchId, ConfigureThresholdResource resource, CancellationToken cancellationToken)
    {
        var command = ConfigureThresholdCommandFromResourceAssembler.ToCommandFromResource(ranchId, resource);
        if (command.IsFailure)
        {
            return OperationsMonitoringActionResultAssembler.ToProblem(this, command.DomainError, localizer);
        }

        var result = await thresholdCommandService.Handle(command.Value!, cancellationToken);
        return OperationsMonitoringActionResultAssembler.ToActionResult(this, result, localizer,
            threshold => Ok(ThresholdResourceFromEntityAssembler.ToResourceFromEntity(threshold)));
    }

    [HttpGet]
    [EndpointSummary("List the thresholds of a ranch")]
    [EndpointDescription("Lists the thresholds configured by an authorized ranch.")]
    [ProducesResponseType(typeof(IReadOnlyList<ThresholdResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByRanch(Guid ranchId, CancellationToken cancellationToken)
    {
        var result = await thresholdQueryService.Handle(new GetThresholdsByRanchQuery(ranchId), cancellationToken);
        return OperationsMonitoringActionResultAssembler.ToActionResult(this, result, localizer,
            thresholds => Ok(thresholds.Select(ThresholdResourceFromEntityAssembler.ToResourceFromEntity).ToList()));
    }
}
