using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;

/// <summary>
///     Controller of the livestock operations.
/// </summary>
/// <param name="operationCommandService">The livestock operation command service.</param>
/// <param name="operationQueryService">The livestock operation query service.</param>
/// <param name="logger">Logger for diagnostics and error reporting.</param>
[ApiController]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Livestock Operations")]
public class LivestockOperationsController(
    ILivestockOperationCommandService operationCommandService,
    ILivestockOperationQueryService operationQueryService,
    ILogger<LivestockOperationsController> logger) : ControllerBase
{
    /// <summary>
    ///     Registers a field event observed by an operator. A retry with the same operation returns 200.
    /// </summary>
    /// <param name="resource">The field event data.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A response as an action result containing the stored operation, or the failure of the request.</returns>
    [HttpPost("api/v1/field-events")]
    [ProducesResponseType(typeof(LivestockOperationResource), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(LivestockOperationResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterFieldEvent(RegisterLivestockOperationResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var command = RegisterFieldEventCommandFromResourceAssembler.ToCommandFromResource(resource);
            var result = await operationCommandService.Handle(command, cancellationToken);
            return OperationsMonitoringActionResultAssembler.ToActionResultFromRegisterLivestockOperationResult(
                this,
                result,
                registration => registration.Created
                    ? Created($"/api/v1/animals/{registration.Operation.CattleId}/livestock-operations",
                        LivestockOperationResourceFromEntityAssembler.ToResourceFromEntity(registration.Operation))
                    : Ok(LivestockOperationResourceFromEntityAssembler.ToResourceFromEntity(registration.Operation)));
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Rejected an invalid request: {Reason}", ex.Message);
            return Problem(title: "The request is not valid.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    /// <summary>
    ///     Registers a clinical intervention applied by an authorized veterinarian. A retry with the same operation returns 200.
    /// </summary>
    /// <param name="resource">The clinical intervention data.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A response as an action result containing the stored operation, or the failure of the request.</returns>
    [HttpPost("api/v1/clinical-interventions")]
    [ProducesResponseType(typeof(LivestockOperationResource), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(LivestockOperationResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterClinicalIntervention(RegisterLivestockOperationResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var command = RegisterClinicalInterventionCommandFromResourceAssembler.ToCommandFromResource(resource);
            var result = await operationCommandService.Handle(command, cancellationToken);
            return OperationsMonitoringActionResultAssembler.ToActionResultFromRegisterLivestockOperationResult(
                this,
                result,
                registration => registration.Created
                    ? Created($"/api/v1/animals/{registration.Operation.CattleId}/livestock-operations",
                        LivestockOperationResourceFromEntityAssembler.ToResourceFromEntity(registration.Operation))
                    : Ok(LivestockOperationResourceFromEntityAssembler.ToResourceFromEntity(registration.Operation)));
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Rejected an invalid request: {Reason}", ex.Message);
            return Problem(title: "The request is not valid.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    /// <summary>
    ///     Synchronizes operations recorded offline by the mobile application, with an individual result per operation.
    /// </summary>
    /// <param name="resource">The recorded operations.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A response as an action result containing one result per operation, or the failure of the request.</returns>
    [HttpPost("api/v1/mobile-sync/batch")]
    [ProducesResponseType(typeof(SyncBatchResultResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SynchronizeMobileBatch(MobileSyncBatchResource resource, CancellationToken cancellationToken)
    {
        var command = SynchronizeMobileBatchCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await operationCommandService.Handle(command, cancellationToken);
        return OperationsMonitoringActionResultAssembler.ToActionResultFromSynchronizeMobileBatchResult(
            this,
            result,
            entryResults => Ok(SyncBatchResultResourceFromEntityAssembler.ToResourceFromEntity(entryResults)));
    }

    /// <summary>
    ///     Returns the field events and clinical interventions of an authorized animal within the half-open period.
    /// </summary>
    /// <param name="animalId">The animal the operations were applied to.</param>
    /// <param name="from">The first instant of the period.</param>
    /// <param name="to">The instant that ends the period, excluded from it.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A response as an action result containing the operations, or the failure of the request.</returns>
    [HttpGet("api/v1/animals/{animalId:guid}/livestock-operations")]
    [ProducesResponseType(typeof(IReadOnlyList<LivestockOperationResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistory(
        Guid animalId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        try
        {
            var period = new TimePeriod(
                from ?? throw new ArgumentException("Both from and to ISO 8601 timestamps are required."),
                to ?? throw new ArgumentException("Both from and to ISO 8601 timestamps are required."));
            var result = await operationQueryService.Handle(new GetLivestockOperationsQuery(animalId, period), cancellationToken);
            return OperationsMonitoringActionResultAssembler.ToActionResultFromGetLivestockOperationsResult(
                this,
                result,
                operations => Ok(operations.Select(LivestockOperationResourceFromEntityAssembler.ToResourceFromEntity).ToList()));
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Rejected an invalid request: {Reason}", ex.Message);
            return Problem(title: "The request is not valid.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
