using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
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
///     Registers field events and clinical interventions, synchronizes offline mobile batches and lists the operations of animals.
/// </summary>
[ApiController]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Livestock Operations")]
public class LivestockOperationsController(
    ILivestockOperationCommandService operationCommandService,
    ILivestockOperationQueryService operationQueryService,
    IStringLocalizer<OperationsMonitoringMessages> localizer) : ControllerBase
{
    [HttpPost("api/v1/field-events")]
    [EndpointSummary("Register a field event")]
    [EndpointDescription("Registers a field event observed by an operator. A retry with the same operation returns 200 with the stored record.")]
    [ProducesResponseType(typeof(LivestockOperationResource), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(LivestockOperationResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterFieldEvent(RegisterLivestockOperationResource resource, CancellationToken cancellationToken)
    {
        var command = RegisterFieldEventCommandFromResourceAssembler.ToCommandFromResource(resource);
        if (command.IsFailure)
        {
            return OperationsMonitoringActionResultAssembler.ToProblem(this, command.DomainError, localizer);
        }

        var result = await operationCommandService.Handle(command.Value!, cancellationToken);
        return OperationsMonitoringActionResultAssembler.ToActionResult(this, result, localizer, RegistrationResponse);
    }

    [HttpPost("api/v1/clinical-interventions")]
    [EndpointSummary("Register a clinical intervention")]
    [EndpointDescription("Registers a clinical intervention applied by an authorized veterinarian. A retry with the same operation returns 200 with the stored record.")]
    [ProducesResponseType(typeof(LivestockOperationResource), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(LivestockOperationResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterClinicalIntervention(RegisterLivestockOperationResource resource, CancellationToken cancellationToken)
    {
        var command = RegisterClinicalInterventionCommandFromResourceAssembler.ToCommandFromResource(resource);
        if (command.IsFailure)
        {
            return OperationsMonitoringActionResultAssembler.ToProblem(this, command.DomainError, localizer);
        }

        var result = await operationCommandService.Handle(command.Value!, cancellationToken);
        return OperationsMonitoringActionResultAssembler.ToActionResult(this, result, localizer, RegistrationResponse);
    }

    [HttpPost("api/v1/mobile-sync/batch")]
    [EndpointSummary("Synchronize a mobile batch")]
    [EndpointDescription("Synchronizes operations recorded offline by the mobile application, with an individual result per operation.")]
    [ProducesResponseType(typeof(SyncBatchResultResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SynchronizeMobileBatch(MobileSyncBatchResource resource, CancellationToken cancellationToken)
    {
        var command = SynchronizeMobileBatchCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await operationCommandService.Handle(command, cancellationToken);
        return OperationsMonitoringActionResultAssembler.ToActionResult(this, result, localizer,
            entryResults => Ok(SyncBatchResultResourceFromEntityAssembler.ToResourceFromEntity(entryResults)));
    }

    [HttpGet("api/v1/animals/{animalId:guid}/livestock-operations")]
    [EndpointSummary("List the livestock operations of an animal")]
    [EndpointDescription("Returns the field events and clinical interventions of an authorized animal within the half-open period.")]
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
        var period = TimePeriodFromRequestAssembler.ToPeriodFromRequest(from, to);
        if (period.IsFailure)
        {
            return OperationsMonitoringActionResultAssembler.ToProblem(this, period.DomainError, localizer);
        }

        var result = await operationQueryService.Handle(new GetLivestockOperationsQuery(animalId, period.Value!), cancellationToken);
        return OperationsMonitoringActionResultAssembler.ToActionResult(this, result, localizer,
            operations => Ok(operations.Select(LivestockOperationResourceFromEntityAssembler.ToResourceFromEntity).ToList()));
    }

    private IActionResult RegistrationResponse(LivestockOperationRegistration registration) =>
        registration.Created
            ? Created($"/api/v1/animals/{registration.Operation.CattleId}/livestock-operations",
                LivestockOperationResourceFromEntityAssembler.ToResourceFromEntity(registration.Operation))
            : Ok(LivestockOperationResourceFromEntityAssembler.ToResourceFromEntity(registration.Operation));
}
