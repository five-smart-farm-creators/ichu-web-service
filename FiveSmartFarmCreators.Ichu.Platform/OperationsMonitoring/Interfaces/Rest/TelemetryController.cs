using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Services;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;

/// <summary>
///     Controller of the telemetry that Edge Gateways synchronize.
/// </summary>
/// <param name="synchronizationCommandService">The Edge Gateway synchronization command service.</param>
/// <param name="readingQueryService">The telemetry reading query service.</param>
[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Telemetry")]
public class TelemetryController(
    IEdgeSynchronizationCommandService synchronizationCommandService,
    ITelemetryReadingQueryService readingQueryService) : ControllerBase
{
    /// <summary>
    ///     Receives readings, alerts and alert changes from an Edge Gateway with an individual result per entry.
    ///     Gateway credentials are verified by the Edge Gateway authenticator, not by user authentication.
    /// </summary>
    /// <param name="resource">The batch sent by the gateway.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A response as an action result containing one result per entry, or the failure of the request.</returns>
    [HttpPost("api/v1/telemetry/batch")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SyncBatchResultResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> SynchronizeBatch(EdgeBatchResource resource, CancellationToken cancellationToken)
    {
        var command = SynchronizeEdgeBatchCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await synchronizationCommandService.Handle(command, cancellationToken);
        return ActionResultFromSynchronizeEdgeBatchResultAssembler.ToActionResultFromSynchronizeEdgeBatchResult(result, this);
    }

    /// <summary>
    ///     Returns the readings of an authorized animal captured within the half-open period, ordered by capture time.
    /// </summary>
    /// <param name="animalId">The animal the readings belong to.</param>
    /// <param name="from">The first instant of the period.</param>
    /// <param name="to">The instant that ends the period, excluded from it.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of readings per page.</param>
    /// <returns>A response as an action result containing the readings, or the failure of the request.</returns>
    [HttpGet("api/v1/animals/{animalId:guid}/telemetry")]
    [Authorize]
    [ProducesResponseType(typeof(TelemetryHistoryResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetHistory(
        Guid animalId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        try
        {
            var period = new TimePeriod(
                from ?? throw new ArgumentException("Both from and to ISO 8601 timestamps are required."),
                to ?? throw new ArgumentException("Both from and to ISO 8601 timestamps are required."));
            var result = await readingQueryService.Handle(new GetAnimalTelemetryQuery(animalId, period, page, pageSize), cancellationToken);
            return ActionResultFromGetAnimalTelemetryResultAssembler.ToActionResultFromGetAnimalTelemetryResult(result, this);
        }
        catch (ArgumentException ex)
        {
            return Problem(title: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    /// <summary>
    ///     Returns the latest valid value of each indicator and the last known position, with their capture times.
    /// </summary>
    /// <param name="animalId">The animal the readings belong to.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A response as an action result containing the latest indicators, or the failure of the request.</returns>
    [HttpGet("api/v1/animals/{animalId:guid}/telemetry/latest")]
    [Authorize]
    [ProducesResponseType(typeof(LatestTelemetryResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetLatest(Guid animalId, CancellationToken cancellationToken)
    {
        var result = await readingQueryService.Handle(new GetLatestAnimalTelemetryQuery(animalId), cancellationToken);
        return ActionResultFromGetLatestAnimalTelemetryResultAssembler.ToActionResultFromGetLatestAnimalTelemetryResult(result, this);
    }
}
