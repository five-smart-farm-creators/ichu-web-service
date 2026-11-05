using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;

[ApiController]
[Produces("application/json")]
[TypeFilter(typeof(OperationsMonitoringExceptionFilter))]
public sealed class TelemetryController(EdgeSynchronizationService synchronization, TelemetryQueryService telemetry) : ControllerBase
{
    /// <summary>Receives readings, alerts and alert changes from an Edge Gateway with an individual result per entry.</summary>
    /// <remarks>Gateway credentials are verified by the Edge Gateway authenticator, not by user authentication.</remarks>
    [HttpPost("api/v1/telemetry/batch")]
    [AllowAnonymous]
    public async Task<ActionResult<SyncBatchResultResource>> SynchronizeBatch(EdgeBatchResource resource, CancellationToken cancellationToken)
    {
        var results = await synchronization.SynchronizeAsync(resource.ToCommand(), cancellationToken);
        return Ok(SyncBatchResultResource.From(results));
    }

    /// <summary>Returns the readings of an authorized animal captured within the half-open period, ordered by capture time.</summary>
    [HttpGet("api/v1/animals/{animalId:guid}/telemetry")]
    [Authorize]
    public async Task<ActionResult<TelemetryHistoryResource>> GetHistory(
        Guid animalId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var query = new GetAnimalTelemetryQuery(animalId, PeriodParameters.Require(from, to), page, pageSize);
        return Ok(TelemetryHistoryResource.FromHistory(await telemetry.GetHistoryAsync(query, cancellationToken)));
    }

    /// <summary>Returns the latest valid value of each indicator and the last known position, with their capture times.</summary>
    [HttpGet("api/v1/animals/{animalId:guid}/telemetry/latest")]
    [Authorize]
    public async Task<ActionResult<LatestTelemetryResource>> GetLatest(Guid animalId, CancellationToken cancellationToken) =>
        Ok(LatestTelemetryResource.From(await telemetry.GetLatestAsync(animalId, cancellationToken)));
}
