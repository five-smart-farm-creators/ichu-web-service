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

[ApiController]
[Route("api/v1/ranches/{ranchId:guid}/water-troughs")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Water monitoring")]
public sealed class WaterTroughMonitoringController(
    IWaterTroughMonitoringQueryService monitoringQueryService,
    IStringLocalizer<OperationsMonitoringMessages> localizer) : ControllerBase
{
    [HttpGet("status")]
    [EndpointSummary("List the water trough status of a ranch")]
    [EndpointDescription("Requires a user bearer token and Read access to ranchId. Returns all troughs ordered by name then UUID. controllerStatus is NoController, NoReadings, Communicating, or NoCommunication; the last value means more than six hours since the last backend receipt (exactly six hours is Communicating). measurementStatus is Unavailable, Invalid, or Valid. rangeStatus is NotConfigured, Unavailable, BelowRange, WithinRange, or AboveRange; configured endpoints are inclusive. evaluatedAt and lastCommunicationAt are server UTC. latestReading is nullable and preserves capture and receipt times; absent data is never zero. Errors: 400 RanchIdentifierRequired; 401 Unauthenticated; 403 Forbidden.")]
    [ProducesResponseType(typeof(WaterTroughStatusesResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStatuses(Guid ranchId, CancellationToken cancellationToken)
    {
        var result = await monitoringQueryService.Handle(new GetWaterTroughStatusesQuery(ranchId), cancellationToken);
        return OperationsMonitoringActionResultAssembler.ToActionResult(this, result, localizer, items =>
            Ok(new WaterTroughStatusesResource(ranchId,
                items.Select(WaterTroughStatusResourceFromEntityAssembler.ToResourceFromEntity).ToArray())));
    }

    [HttpGet("{waterTroughId:guid}/status")]
    [EndpointSummary("Get one water trough status")]
    [EndpointDescription("Requires a user bearer token and Read access to ranchId. Returns the same controllerStatus, measurementStatus, and rangeStatus enums as the ranch list, plus an optional latestReading with capture/receipt timestamps. A trough without a controller or readings remains distinguishable; absent or invalid temperatures are never zero. Errors: 400 RanchIdentifierRequired; 401 Unauthenticated; 403 Forbidden; 404 WaterTroughNotFound.")]
    [ProducesResponseType(typeof(WaterTroughStatusResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(Guid ranchId, Guid waterTroughId, CancellationToken cancellationToken)
    {
        var result = await monitoringQueryService.Handle(new GetWaterTroughStatusQuery(ranchId, waterTroughId),
            cancellationToken);
        return OperationsMonitoringActionResultAssembler.ToActionResult(this, result, localizer,
            status => Ok(WaterTroughStatusResourceFromEntityAssembler.ToResourceFromEntity(status)));
    }
}
