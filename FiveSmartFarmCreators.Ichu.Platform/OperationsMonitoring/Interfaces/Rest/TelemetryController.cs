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
///     Receives the telemetry that Edge Gateways synchronize and returns the readings and latest indicators of animals.
/// </summary>
[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Telemetry")]
public class TelemetryController(
    IEdgeSynchronizationCommandService synchronizationCommandService,
    ITelemetryReadingQueryService readingQueryService,
    IStringLocalizer<OperationsMonitoringMessages> localizer) : ControllerBase
{
    [HttpPost("api/v1/telemetry/batch")]
    [EndpointSummary("Synchronize an Edge Gateway batch")]
    [EndpointDescription("Accepts readings, alerts and alertChanges using the required X-Gateway-Key header in the form {gatewayId}.{secret}. The key identifies the ranch; a Firebase bearer token is not required. Readings carry heartRate in beats per minute (30 to 200) and methaneIndex as a relative index from 0 to 100, not ppm. Returns HTTP 200 with a result per entry: Accepted, Duplicate, Rejected or Conflict and a reason when applicable. A repeated identifier does not create another record. Inspect each entry even when the batch returns 200. Invalid batches return 400 and missing or invalid gateway credentials return 401.")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SyncBatchResultResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SynchronizeBatch(EdgeBatchResource resource, CancellationToken cancellationToken)
    {
        var command = SynchronizeEdgeBatchCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await synchronizationCommandService.Handle(command, cancellationToken);
        return OperationsMonitoringActionResultAssembler.ToActionResult(this, result, localizer,
            entryResults => Ok(SyncBatchResultResourceFromEntityAssembler.ToResourceFromEntity(entryResults)));
    }

    [HttpGet("api/v1/ranches/{ranchId:guid}/cattle/{cattleId:guid}/telemetry")]
    [EndpointSummary("List the telemetry of an animal")]
    [EndpointDescription("Returns the readings of an authorized animal captured within the half-open period, ordered by capture time.")]
    [Authorize]
    [ProducesResponseType(typeof(TelemetryHistoryResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistory(
        Guid ranchId,
        Guid cattleId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var period = TimePeriodFromRequestAssembler.ToPeriodFromRequest(from, to);
        if (period.IsFailure)
        {
            return OperationsMonitoringActionResultAssembler.ToProblem(this, period.DomainError, localizer);
        }

        var result = await readingQueryService.Handle(new GetAnimalTelemetryQuery(ranchId, cattleId, period.Value!, page, pageSize), cancellationToken);
        return OperationsMonitoringActionResultAssembler.ToActionResult(this, result, localizer,
            history => Ok(TelemetryHistoryResourceFromEntityAssembler.ToResourceFromEntity(history)));
    }

    [HttpGet("api/v1/ranches/{ranchId:guid}/cattle/{cattleId:guid}/telemetry/latest")]
    [EndpointSummary("Get the latest telemetry of an animal")]
    [EndpointDescription("Returns the latest valid value of each indicator and the last known position of an authorized animal, with their capture times.")]
    [Authorize]
    [ProducesResponseType(typeof(LatestTelemetryResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatest(Guid ranchId, Guid cattleId, CancellationToken cancellationToken)
    {
        var result = await readingQueryService.Handle(new GetLatestAnimalTelemetryQuery(ranchId, cattleId), cancellationToken);
        return OperationsMonitoringActionResultAssembler.ToActionResult(this, result, localizer,
            latest => Ok(LatestTelemetryResourceFromEntityAssembler.ToResourceFromEntity(latest)));
    }
}
