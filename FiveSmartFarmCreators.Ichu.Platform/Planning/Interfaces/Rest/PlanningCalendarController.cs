using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Transform;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
    [EndpointDescription("Requires read access to ranchId and both from and to query parameters as ISO 8601 UTC instants, for example 2026-11-01T00:00:00Z and 2026-11-30T23:59:59Z. Returns activities scheduled inside this inclusive interval, ordered by schedule, or an empty array. Missing bounds, non-UTC instants and inverted ranges return 400.")]
    [ProducesResponseType<IReadOnlyList<PlanningActivityResource>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCalendarAsync(
        Guid ranchId,
        [FromQuery, BindRequired] DateTimeOffset from,
        [FromQuery, BindRequired] DateTimeOffset to,
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
