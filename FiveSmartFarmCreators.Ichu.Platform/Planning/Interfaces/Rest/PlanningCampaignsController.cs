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
///     Exposes the planning campaigns of the ranches the caller may access.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/ranches/{ranchId:guid}/planning-campaigns")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class PlanningCampaignsController(
    IPlanningCampaignCommandService commandService,
    IPlanningCampaignQueryService queryService,
    IStringLocalizer<PlanningMessages> localizer) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List planning campaigns in an authorized ranch")]
    [EndpointDescription("Returns the campaigns of the ranch ordered by their start date.")]
    [ProducesResponseType<IReadOnlyList<PlanningCampaignResource>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCampaignsAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        var result = await queryService.HandleAsync(new GetPlanningCampaignsByRanchQuery(ranchId), cancellationToken);
        if (result.IsFailure)
        {
            return PlanningActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(result.Value!.Select(PlanningCampaignResourceFromEntityAssembler.ToResourceFromEntity).ToList());
    }

    [HttpGet("~/api/v1/planning-campaigns/{campaignId:guid}")]
    [EndpointSummary("Get a planning campaign")]
    [EndpointDescription("Returns a campaign when the account may read its ranch.")]
    [ProducesResponseType<PlanningCampaignResource>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCampaignAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        var result = await queryService.HandleAsync(
            new GetPlanningCampaignByIdQuery(new PlanningCampaignId(campaignId)), cancellationToken);
        if (result.IsFailure)
        {
            return PlanningActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(PlanningCampaignResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    [HttpPost]
    [EndpointSummary("Create a planning campaign")]
    [EndpointDescription("Creates a sanitary or reproductive campaign in a ranch the account may manage.")]
    [ProducesResponseType<PlanningCampaignResource>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateCampaignAsync(
        Guid ranchId,
        [FromBody] CreatePlanningCampaignResource resource,
        CancellationToken cancellationToken)
    {
        var command = CreatePlanningCampaignCommandFromResourceAssembler.ToCommandFromResource(ranchId, resource);
        var result = await commandService.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return PlanningActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        var campaign = PlanningCampaignResourceFromEntityAssembler.ToResourceFromEntity(result.Value!);
        return Created($"/api/v1/planning-campaigns/{campaign.Id}", campaign);
    }
}
