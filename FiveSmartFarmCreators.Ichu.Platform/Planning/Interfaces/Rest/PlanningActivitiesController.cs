using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Services;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest;

/// <summary>
/// Exposes planned activity resources and their application and follow-up records.
/// </summary>
[Route("api/v1/planned-activities")]
public sealed class PlanningActivitiesController : PlanningControllerBase
{
    private readonly IPlanningApplicationService _planningService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanningActivitiesController"/> class.
    /// </summary>
    /// <param name="planningService">Planning application use cases.</param>
    public PlanningActivitiesController(IPlanningApplicationService planningService)
    {
        _planningService = planningService;
    }

    /// <summary>
    /// Gets a planned activity by identifier.
    /// </summary>
    /// <param name="activityId">The activity identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authorized planned activity.</returns>
    [HttpGet("{activityId:guid}")]
    [ProducesResponseType(typeof(PlanningActivityResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<PlanningActivityResource>> GetActivity(Guid activityId, CancellationToken cancellationToken)
    {
        return ExecuteAsync(async () =>
            PlanningResourceMapper.ToResource(await _planningService.GetActivityAsync(activityId, cancellationToken)));
    }

    /// <summary>
    /// Lists applications recorded for a planned activity.
    /// </summary>
    /// <param name="activityId">The planned activity identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authorized application records.</returns>
    [HttpGet("{activityId:guid}/applications")]
    [ProducesResponseType(typeof(IReadOnlyList<ApplicationRecordResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult<IReadOnlyList<ApplicationRecordResource>>> GetApplications(
        Guid activityId,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync<IReadOnlyList<ApplicationRecordResource>>(async () =>
        {
            var applications = await _planningService.GetApplicationsAsync(activityId, cancellationToken);
            return applications.Select(PlanningResourceMapper.ToResource).ToArray();
        });
    }

    /// <summary>
    /// Schedules a planned activity.
    /// </summary>
    /// <param name="resource">The activity data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The scheduled activity.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PlanningActivityResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult<PlanningActivityResource>> ScheduleActivity(
        [FromBody] SchedulePlanningActivityResource resource,
        CancellationToken cancellationToken)
    {
        var command = new SchedulePlanningActivityCommand(
            resource.RanchId,
            resource.CampaignId,
            resource.CattleId,
            resource.Title,
            resource.ScheduledAt);

        return ExecuteCreatedAsync(
            async () => PlanningResourceMapper.ToResource(
                await _planningService.ScheduleActivityAsync(command, cancellationToken)),
            activity => $"/api/v1/planned-activities/{activity.Id}");
    }

    /// <summary>
    /// Records an application associated with a planned activity.
    /// </summary>
    /// <param name="activityId">The planned activity identifier.</param>
    /// <param name="resource">The application data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The application record.</returns>
    [HttpPost("{activityId:guid}/applications")]
    [ProducesResponseType(typeof(ApplicationRecordResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult<ApplicationRecordResource>> RecordApplication(
        Guid activityId,
        [FromBody] RecordApplicationResource resource,
        CancellationToken cancellationToken)
    {
        var command = new RecordApplicationCommand(
            activityId,
            resource.CattleId,
            resource.AppliedAt,
            resource.InterventionId);

        return ExecuteCreatedAsync(
            async () => PlanningResourceMapper.ToResource(
                await _planningService.RecordApplicationAsync(command, cancellationToken)),
            application => $"/api/v1/planned-activities/{application.ActivityId}/applications");
    }

    /// <summary>
    /// Schedules a follow-up for a planned activity.
    /// </summary>
    /// <param name="activityId">The planned activity identifier.</param>
    /// <param name="resource">The follow-up data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The scheduled follow-up.</returns>
    [HttpPost("{activityId:guid}/follow-ups")]
    [ProducesResponseType(typeof(FollowUpResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult<FollowUpResource>> ScheduleFollowUp(
        Guid activityId,
        [FromBody] ScheduleFollowUpResource resource,
        CancellationToken cancellationToken)
    {
        var command = new ScheduleFollowUpCommand(activityId, resource.DueAt);

        return ExecuteCreatedAsync(
            async () => PlanningResourceMapper.ToResource(
                await _planningService.ScheduleFollowUpAsync(command, cancellationToken)),
            followUp => $"/api/v1/planned-activities/{followUp.ActivityId}/follow-ups");
    }

    /// <summary>
    /// Lists follow-ups scheduled for a planned activity.
    /// </summary>
    /// <param name="activityId">The planned activity identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authorized follow-ups.</returns>
    [HttpGet("{activityId:guid}/follow-ups")]
    [ProducesResponseType(typeof(IReadOnlyList<FollowUpResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult<IReadOnlyList<FollowUpResource>>> GetFollowUps(
        Guid activityId,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync<IReadOnlyList<FollowUpResource>>(async () =>
        {
            var followUps = await _planningService.GetFollowUpsAsync(activityId, cancellationToken);
            return followUps.Select(PlanningResourceMapper.ToResource).ToArray();
        });
    }
}
