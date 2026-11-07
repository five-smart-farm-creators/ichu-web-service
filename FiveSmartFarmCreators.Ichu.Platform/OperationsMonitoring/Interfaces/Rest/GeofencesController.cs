using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;

/// <summary>
///     Geofences' controller.
/// </summary>
/// <param name="geofenceCommandService">The geofence command service.</param>
/// <param name="geofenceQueryService">The geofence query service.</param>
/// <param name="logger">Logger for diagnostics and error reporting.</param>
[ApiController]
[Route("api/v1/geofences")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Geofences")]
public class GeofencesController(
    IGeofenceCommandService geofenceCommandService,
    IGeofenceQueryService geofenceQueryService,
    ILogger<GeofencesController> logger) : ControllerBase
{
    /// <summary>
    ///     Defines the permitted grazing area of a lot. A lot has at most one geofence.
    /// </summary>
    /// <param name="resource">The geofence definition.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A response as an action result containing the created geofence, or the failure of the request.</returns>
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
        try
        {
            var command = DefineGeofenceCommandFromResourceAssembler.ToCommandFromResource(resource);
            var result = await geofenceCommandService.Handle(command, cancellationToken);
            return OperationsMonitoringActionResultAssembler.ToActionResultFromDefineGeofenceResult(
                this,
                result,
                geofence => Created($"/api/v1/geofences?ranchId={geofence.RanchId}",
                    GeofenceResourceFromEntityAssembler.ToResourceFromEntity(geofence)));
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Rejected an invalid request: {Reason}", ex.Message);
            return Problem(title: "The request is not valid.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    /// <summary>
    ///     Replaces the name, tolerance, severity and boundary of an existing geofence.
    /// </summary>
    /// <param name="geofenceId">The geofence to redefine.</param>
    /// <param name="resource">The new geofence definition.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A response as an action result containing the stored geofence, or the failure of the request.</returns>
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
        try
        {
            var command = RedefineGeofenceCommandFromResourceAssembler.ToCommandFromResource(geofenceId, resource);
            var result = await geofenceCommandService.Handle(command, cancellationToken);
            return OperationsMonitoringActionResultAssembler.ToActionResultFromRedefineGeofenceResult(
                this,
                result,
                geofence => Ok(GeofenceResourceFromEntityAssembler.ToResourceFromEntity(geofence)));
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Rejected an invalid request: {Reason}", ex.Message);
            return Problem(title: "The request is not valid.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    /// <summary>
    ///     Lists the geofences defined by an authorized ranch.
    /// </summary>
    /// <param name="ranchId">The ranch that owns the geofences.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A response as an action result containing the geofences, or the failure of the request.</returns>
    [HttpGet]
    [EndpointSummary("List the geofences of a ranch")]
    [EndpointDescription("Lists the geofences defined by an authorized ranch.")]
    [ProducesResponseType(typeof(IReadOnlyList<GeofenceResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByRanch([FromQuery] Guid ranchId, CancellationToken cancellationToken)
    {
        var result = await geofenceQueryService.Handle(new GetGeofencesByRanchQuery(ranchId), cancellationToken);
        return OperationsMonitoringActionResultAssembler.ToActionResultFromGetGeofencesResult(
            this,
            result,
            geofences => Ok(geofences.Select(GeofenceResourceFromEntityAssembler.ToResourceFromEntity).ToList()));
    }
}
