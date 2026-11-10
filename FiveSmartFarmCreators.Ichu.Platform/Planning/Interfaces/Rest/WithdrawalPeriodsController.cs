using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Transform;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest;

/// <summary>
///     Exposes withdrawal periods whose bounds an authorized caller supplies explicitly.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/ranches/{ranchId:guid}/withdrawal-periods")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class WithdrawalPeriodsController(
    IWithdrawalPeriodCommandService commandService,
    IWithdrawalPeriodQueryService queryService,
    IStringLocalizer<PlanningMessages> localizer) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List withdrawal periods in an authorized ranch")]
    [EndpointDescription("Returns the withdrawal periods of the ranch, optionally narrowed to one animal of the same ranch.")]
    [ProducesResponseType<IReadOnlyList<WithdrawalPeriodResource>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWithdrawalPeriodsAsync(
        Guid ranchId,
        [FromQuery] Guid? cattleId,
        CancellationToken cancellationToken)
    {
        var result = await queryService.HandleAsync(new GetWithdrawalPeriodsQuery(ranchId, cattleId), cancellationToken);
        if (result.IsFailure)
        {
            return PlanningActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(result.Value!.Select(WithdrawalPeriodResourceFromEntityAssembler.ToResourceFromEntity).ToList());
    }

    [HttpPost]
    [EndpointSummary("Record a withdrawal period")]
    [EndpointDescription("Records a withdrawal interval with explicit UTC bounds supplied by an authorized caller. Planning does not calculate it.")]
    [ProducesResponseType<WithdrawalPeriodResource>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RecordWithdrawalPeriodAsync(
        Guid ranchId,
        [FromBody] RecordWithdrawalPeriodResource resource,
        CancellationToken cancellationToken)
    {
        var command = RecordWithdrawalPeriodCommandFromResourceAssembler.ToCommandFromResource(ranchId, resource);
        var result = await commandService.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return PlanningActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        var period = WithdrawalPeriodResourceFromEntityAssembler.ToResourceFromEntity(result.Value!);
        return Created($"/api/v1/ranches/{ranchId}/withdrawal-periods?cattleId={period.CattleId}", period);
    }
}
