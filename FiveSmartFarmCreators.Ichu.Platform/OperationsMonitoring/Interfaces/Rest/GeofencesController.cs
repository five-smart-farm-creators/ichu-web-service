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
///     Defines, redefines and lists the geofences of lots.
/// </summary>
[ApiController]
[Route("api/v1/geofences")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Geofences")]
public class GeofencesController(
    IGeofenceCommandService geofenceCommandService,
    IGeofenceQueryService geofenceQueryService,
    IStringLocalizer<OperationsMonitoringMessages> localizer) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Define a geofence")]
    [EndpointDescription("Defines the permitted grazing area of a lot. A lot has at most one geofence.")]
    [ProducesResponseType(typeof(GeofenceResource), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Define(DefineGeofenceResource resource, CancellationToken cancellationToken)
    {
        var command = DefineGeofenceCommandFromResourceAssembler.ToCommandFromResource(resource);
        if (command.IsFailure)
        {
            return OperationsMonitoringActionResultAssembler.ToProblem(this, command.DomainError, localizer);
        }

        var result = await geofenceCommandService.Handle(command.Value!, cancellationToken);
        return OperationsMonitoringActionResultAssembler.ToActionResult(this, result, localizer,
            geofence => Created($"/api/v1/geofences?ranchId={geofence.RanchId}",
                GeofenceResourceFromEntityAssembler.ToResourceFromEntity(geofence)));
    }

    [HttpPut("{geofenceId:guid}")]
    [EndpointSummary("Redefine a geofence")]
    [EndpointDescription("Replaces the name, tolerance, severity and boundary of an existing geofence.")]
    [ProducesResponseType(typeof(GeofenceResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Redefine(Guid geofenceId, RedefineGeofenceResource resource, CancellationToken cancellationToken)
    {
        var command = RedefineGeofenceCommandFromResourceAssembler.ToCommandFromResource(geofenceId, resource);
        if (command.IsFailure)
        {
            return OperationsMonitoringActionResultAssembler.ToProblem(this, command.DomainError, localizer);
        }

        var result = await geofenceCommandService.Handle(command.Value!, cancellationToken);
        return OperationsMonitoringActionResultAssembler.ToActionResult(this, result, localizer,
            geofence => Ok(GeofenceResourceFromEntityAssembler.ToResourceFromEntity(geofence)));
    }

    [HttpGet]
    [EndpointSummary("List the geofences of a ranch")]
    [EndpointDescription("Lists the geofences defined by an authorized ranch.")]
    [ProducesResponseType(typeof(IReadOnlyList<GeofenceResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByRanch([FromQuery] Guid ranchId, CancellationToken cancellationToken)
    {
        var result = await geofenceQueryService.Handle(new GetGeofencesByRanchQuery(ranchId), cancellationToken);
        return OperationsMonitoringActionResultAssembler.ToActionResult(this, result, localizer,
            geofences => Ok(geofences.Select(GeofenceResourceFromEntityAssembler.ToResourceFromEntity).ToList()));
    }
}
