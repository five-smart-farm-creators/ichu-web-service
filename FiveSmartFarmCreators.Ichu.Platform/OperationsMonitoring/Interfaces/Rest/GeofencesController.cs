using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;

[ApiController]
[Route("api/v1/geofences")]
[Authorize]
[Produces("application/json")]
[TypeFilter(typeof(OperationsMonitoringExceptionFilter))]
public sealed class GeofencesController(MonitoringRulesService rules) : ControllerBase
{
    /// <summary>Defines the permitted grazing area of a lot. A lot has at most one geofence.</summary>
    [HttpPost]
    public async Task<ActionResult<GeofenceResource>> Define(GeofenceDefinitionResource resource, CancellationToken cancellationToken)
    {
        var geofence = await rules.DefineGeofenceAsync(resource.ToDefineCommand(), cancellationToken);
        return Created($"/api/v1/geofences?ranchId={geofence.RanchId}", GeofenceResource.From(geofence));
    }

    /// <summary>Replaces the name, tolerance, severity and boundary of an existing geofence.</summary>
    [HttpPut("{geofenceId:guid}")]
    public async Task<ActionResult<GeofenceResource>> Redefine(Guid geofenceId, GeofenceDefinitionResource resource, CancellationToken cancellationToken) =>
        Ok(GeofenceResource.From(await rules.RedefineGeofenceAsync(resource.ToRedefineCommand(geofenceId), cancellationToken)));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GeofenceResource>>> GetByRanch([FromQuery] Guid ranchId, CancellationToken cancellationToken) =>
        Ok((await rules.GetGeofencesAsync(ranchId, cancellationToken)).Select(GeofenceResource.From).ToList());
}
