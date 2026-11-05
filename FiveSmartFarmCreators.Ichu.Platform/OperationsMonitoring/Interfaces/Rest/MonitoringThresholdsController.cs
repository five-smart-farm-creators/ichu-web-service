using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;

[ApiController]
[Route("api/v1/monitoring-thresholds")]
[Authorize]
[Produces("application/json")]
[TypeFilter(typeof(OperationsMonitoringExceptionFilter))]
public sealed class MonitoringThresholdsController(MonitoringRulesService rules) : ControllerBase
{
    /// <summary>Creates or replaces the threshold of a ranch for one life stage and metric, advancing the rule revision.</summary>
    [HttpPut]
    public async Task<ActionResult<ThresholdResource>> Configure(ConfigureThresholdResource resource, CancellationToken cancellationToken) =>
        Ok(ThresholdResource.From(await rules.ConfigureThresholdAsync(resource.ToCommand(), cancellationToken)));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ThresholdResource>>> GetByRanch([FromQuery] Guid ranchId, CancellationToken cancellationToken) =>
        Ok((await rules.GetThresholdsAsync(ranchId, cancellationToken)).Select(ThresholdResource.From).ToList());
}
