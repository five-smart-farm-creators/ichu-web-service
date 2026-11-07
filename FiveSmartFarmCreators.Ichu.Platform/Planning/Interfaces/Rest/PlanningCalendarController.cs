using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Services;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest;

/// <summary>
/// Exposes the authorized calendar view of planned activities.
/// </summary>
[Route("api/v1/calendar")]
public sealed class PlanningCalendarController : PlanningControllerBase
{
    private readonly IPlanningApplicationService _planningService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanningCalendarController"/> class.
    /// </summary>
    /// <param name="planningService">Planning application use cases.</param>
    public PlanningCalendarController(IPlanningApplicationService planningService)
    {
        _planningService = planningService;
    }

    /// <summary>
    /// Lists planned activities in an inclusive UTC interval.
    /// </summary>
    /// <param name="ranchId">The ranch identifier.</param>
    /// <param name="from">The first inclusive UTC instant.</param>
    /// <param name="to">The last inclusive UTC instant.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authorized activities in the interval.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PlanningActivityResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult<IReadOnlyList<PlanningActivityResource>>> GetCalendar(
        [FromQuery] Guid ranchId,
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var query = new GetPlanningCalendarQuery(ranchId, from, to);

        return ExecuteAsync<IReadOnlyList<PlanningActivityResource>>(async () =>
        {
            var activities = await _planningService.GetCalendarAsync(query, cancellationToken);
            return activities.Select(PlanningResourceMapper.ToResource).ToArray();
        });
    }
}
