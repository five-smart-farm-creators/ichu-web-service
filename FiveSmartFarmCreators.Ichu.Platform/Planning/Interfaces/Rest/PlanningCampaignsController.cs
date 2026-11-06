using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Services;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest;

/// <summary>
/// Exposes planning campaign resources.
/// </summary>
[Route("api/v1/planning-campaigns")]
public sealed class PlanningCampaignsController : PlanningControllerBase
{
    private readonly IPlanningApplicationService _planningService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanningCampaignsController"/> class.
    /// </summary>
    /// <param name="planningService">Planning application use cases.</param>
    public PlanningCampaignsController(IPlanningApplicationService planningService)
    {
        _planningService = planningService;
    }

    /// <summary>
    /// Lists campaigns in a ranch.
    /// </summary>
    /// <param name="ranchId">The ranch identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authorized campaigns.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PlanningCampaignResource>), StatusCodes.Status200OK)]
    public Task<ActionResult<IReadOnlyList<PlanningCampaignResource>>> GetCampaigns(
        [FromQuery] Guid ranchId,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync<IReadOnlyList<PlanningCampaignResource>>(async () =>
        {
            var campaigns = await _planningService.GetCampaignsAsync(ranchId, cancellationToken);
            return campaigns.Select(PlanningResourceMapper.ToResource).ToArray();
        });
    }

    /// <summary>
    /// Gets a campaign by identifier.
    /// </summary>
    /// <param name="campaignId">The campaign identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authorized campaign.</returns>
    [HttpGet("{campaignId:guid}")]
    [ProducesResponseType(typeof(PlanningCampaignResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<PlanningCampaignResource>> GetCampaign(Guid campaignId, CancellationToken cancellationToken)
    {
        return ExecuteAsync(async () =>
            PlanningResourceMapper.ToResource(await _planningService.GetCampaignAsync(campaignId, cancellationToken)));
    }

    /// <summary>
    /// Creates a campaign.
    /// </summary>
    /// <param name="resource">The campaign data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created campaign.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PlanningCampaignResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult<PlanningCampaignResource>> CreateCampaign(
        [FromBody] CreatePlanningCampaignResource resource,
        CancellationToken cancellationToken)
    {
        var command = new CreatePlanningCampaignCommand(
            resource.RanchId,
            resource.Name,
            resource.Kind,
            resource.StartsOn,
            resource.EndsOn);

        return ExecuteCreatedAsync(
            async () => PlanningResourceMapper.ToResource(
                await _planningService.CreateCampaignAsync(command, cancellationToken)),
            campaign => $"/api/v1/planning-campaigns/{campaign.Id}");
    }
}
