using System.Net.Mime;
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
///     Exposes the planned activities of an authorized ranch inside an inclusive UTC interval.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/ranches/{ranchId:guid}/calendar")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class PlanningCalendarController(
    IPlannedActivityQueryService queryService,
    IStringLocalizer<PlanningMessages> localizer) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List planned activities in a calendar interval")]
    [EndpointDescription("Returns the activities of the ranch scheduled between the two inclusive UTC instants, ordered by their schedule.")]
    [ProducesResponseType<IReadOnlyList<PlanningActivityResource>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCalendarAsync(
        Guid ranchId,
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var result = await queryService.HandleAsync(new GetPlanningCalendarQuery(ranchId, from, to), cancellationToken);
        if (result.IsFailure)
        {
            return PlanningActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(result.Value!.Select(PlannedActivityResourceFromEntityAssembler.ToResourceFromEntity).ToList());
    }
}
