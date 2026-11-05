using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;

[ApiController]
[Authorize]
[Produces("application/json")]
[TypeFilter(typeof(OperationsMonitoringExceptionFilter))]
public sealed class LivestockOperationsController(LivestockOperationService operations) : ControllerBase
{
    /// <summary>Registers a field event observed by an operator. A retry with the same operation returns 200.</summary>
    [HttpPost("api/v1/field-events")]
    public async Task<ActionResult<LivestockOperationResource>> RegisterFieldEvent(RegisterLivestockOperationResource resource, CancellationToken cancellationToken) =>
        ToActionResult(await operations.RegisterFieldEventAsync(resource.ToCommand(), cancellationToken));

    /// <summary>Registers a clinical intervention applied by an authorized veterinarian. A retry with the same operation returns 200.</summary>
    [HttpPost("api/v1/clinical-interventions")]
    public async Task<ActionResult<LivestockOperationResource>> RegisterClinicalIntervention(RegisterLivestockOperationResource resource, CancellationToken cancellationToken) =>
        ToActionResult(await operations.RegisterClinicalInterventionAsync(resource.ToCommand(), cancellationToken));

    /// <summary>Synchronizes operations recorded offline by the mobile application, with an individual result per operation.</summary>
    [HttpPost("api/v1/mobile-sync/batch")]
    public async Task<ActionResult<SyncBatchResultResource>> SynchronizeMobileBatch(MobileSyncBatchResource resource, CancellationToken cancellationToken) =>
        Ok(SyncBatchResultResource.From(await operations.SynchronizeMobileBatchAsync(resource.ToCommands(), cancellationToken)));

    /// <summary>Returns the field events and clinical interventions of an authorized animal within the half-open period.</summary>
    [HttpGet("api/v1/animals/{animalId:guid}/livestock-operations")]
    public async Task<ActionResult<IReadOnlyList<LivestockOperationResource>>> GetHistory(
        Guid animalId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var query = new GetLivestockOperationsQuery(animalId, PeriodParameters.Require(from, to));
        return Ok((await operations.GetHistoryAsync(query, cancellationToken)).Select(LivestockOperationResource.From).ToList());
    }

    private ActionResult<LivestockOperationResource> ToActionResult(LivestockOperationResult result) => result.Status switch
    {
        SyncEntryStatus.Accepted => Created($"/api/v1/animals/{result.Operation!.CattleId}/livestock-operations", LivestockOperationResource.From(result.Operation)),
        SyncEntryStatus.Duplicate => Ok(LivestockOperationResource.From(result.Operation!)),
        _ => Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = result.Reason })
    };
}
