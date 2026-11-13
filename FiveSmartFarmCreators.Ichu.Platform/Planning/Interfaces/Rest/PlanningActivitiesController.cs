using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Transform;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest;

/// <summary>
///     Exposes planned activities with their applications and follow-ups for the accounts that may access their ranch.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/ranches/{ranchId:guid}/planned-activities")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class PlanningActivitiesController(
    IPlannedActivityCommandService commandService,
    IPlannedActivityQueryService queryService,
    IStringLocalizer<PlanningMessages> localizer) : ControllerBase
{
    [HttpGet("~/api/v1/planned-activities/{activityId:guid}")]
    [EndpointSummary("Get a planned activity")]
    [EndpointDescription("Returns a planned activity when the account may read its ranch.")]
    [ProducesResponseType<PlanningActivityResource>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActivityAsync(Guid activityId, CancellationToken cancellationToken)
    {
        var result = await queryService.HandleAsync(
            new GetPlannedActivityByIdQuery(new PlannedActivityId(activityId)), cancellationToken);
        if (result.IsFailure)
        {
            return PlanningActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(PlannedActivityResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    [HttpPost]
    [EndpointSummary("Schedule a planned activity")]
    [EndpointDescription("Schedules an activity for a ranch, or for one of its animals, optionally inside a campaign of the same ranch.")]
    [ProducesResponseType<PlanningActivityResource>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ScheduleActivityAsync(
        Guid ranchId,
        [FromBody] SchedulePlanningActivityResource resource,
        CancellationToken cancellationToken)
    {
        var command = SchedulePlanningActivityCommandFromResourceAssembler.ToCommandFromResource(ranchId, resource);
        var result = await commandService.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return PlanningActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        var activity = PlannedActivityResourceFromEntityAssembler.ToResourceFromEntity(result.Value!);
        return Created($"/api/v1/planned-activities/{activity.Id}", activity);
    }

    [HttpGet("~/api/v1/planned-activities/{activityId:guid}/applications")]
    [EndpointSummary("List the applications of a planned activity")]
    [EndpointDescription("Returns the application records of the activity ordered by their application instant.")]
    [ProducesResponseType<IReadOnlyList<ApplicationRecordResource>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApplicationsAsync(Guid activityId, CancellationToken cancellationToken)
    {
        var result = await queryService.HandleAsync(
            new GetApplicationRecordsByActivityQuery(new PlannedActivityId(activityId)), cancellationToken);
        if (result.IsFailure)
        {
            return PlanningActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(result.Value!.Select(ApplicationRecordResourceFromEntityAssembler.ToResourceFromEntity).ToList());
    }

    [HttpPost("~/api/v1/planned-activities/{activityId:guid}/applications")]
    [EndpointSummary("Record an application of a planned activity")]
    [EndpointDescription("Records that the activity was applied to one of its animals, optionally linked to a clinical intervention of the same animal.")]
    [ProducesResponseType<ApplicationRecordResource>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RecordApplicationAsync(
        Guid activityId,
        [FromBody] RecordApplicationResource resource,
        CancellationToken cancellationToken)
    {
        var command = RecordApplicationCommandFromResourceAssembler.ToCommandFromResource(activityId, resource);
        var result = await commandService.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return PlanningActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        var application = ApplicationRecordResourceFromEntityAssembler.ToResourceFromEntity(result.Value!);
        return Created($"/api/v1/planned-activities/{application.ActivityId}/applications", application);
    }

    [HttpGet("~/api/v1/planned-activities/{activityId:guid}/follow-ups")]
    [EndpointSummary("List the follow-ups of a planned activity")]
    [EndpointDescription("Returns the follow-ups of the activity ordered by their due instant.")]
    [ProducesResponseType<IReadOnlyList<FollowUpResource>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFollowUpsAsync(Guid activityId, CancellationToken cancellationToken)
    {
        var result = await queryService.HandleAsync(
            new GetFollowUpsByActivityQuery(new PlannedActivityId(activityId)), cancellationToken);
        if (result.IsFailure)
        {
            return PlanningActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(result.Value!.Select(FollowUpResourceFromEntityAssembler.ToResourceFromEntity).ToList());
    }

    [HttpPost("~/api/v1/planned-activities/{activityId:guid}/follow-ups")]
    [EndpointSummary("Schedule a follow-up of a planned activity")]
    [EndpointDescription("Schedules a follow-up that falls due at a UTC instant after the activity.")]
    [ProducesResponseType<FollowUpResource>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleFollowUpAsync(
        Guid activityId,
        [FromBody] ScheduleFollowUpResource resource,
        CancellationToken cancellationToken)
    {
        var command = ScheduleFollowUpCommandFromResourceAssembler.ToCommandFromResource(activityId, resource);
        var result = await commandService.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return PlanningActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        var followUp = FollowUpResourceFromEntityAssembler.ToResourceFromEntity(result.Value!);
        return Created($"/api/v1/planned-activities/{activityId}/follow-ups", followUp);
    }
}
