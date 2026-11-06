using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Services;
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
    [ProducesResponseType(typeof(GeofenceResource), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Define(DefineGeofenceResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var command = DefineGeofenceCommandFromResourceAssembler.ToCommandFromResource(resource);
            var result = await geofenceCommandService.Handle(command, cancellationToken);
            return ActionResultFromDefineGeofenceResultAssembler.ToActionResultFromDefineGeofenceResult(result, this);
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
    [ProducesResponseType(typeof(GeofenceResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Redefine(Guid geofenceId, RedefineGeofenceResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var command = RedefineGeofenceCommandFromResourceAssembler.ToCommandFromResource(geofenceId, resource);
            var result = await geofenceCommandService.Handle(command, cancellationToken);
            return ActionResultFromRedefineGeofenceResultAssembler.ToActionResultFromRedefineGeofenceResult(result, this);
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
    [ProducesResponseType(typeof(IReadOnlyList<GeofenceResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> GetByRanch([FromQuery] Guid ranchId, CancellationToken cancellationToken)
    {
        var result = await geofenceQueryService.Handle(new GetGeofencesByRanchQuery(ranchId), cancellationToken);
        return ActionResultFromGetGeofencesResultAssembler.ToActionResultFromGetGeofencesResult(result, this);
    }
}
