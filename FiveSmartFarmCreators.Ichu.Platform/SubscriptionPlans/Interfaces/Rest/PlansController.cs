using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Transform;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest;

/// <summary>
///     Public catalog of the subscription plans and their price estimates, so that a visitor can compare plans before
///     signing in.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Subscription Plans")]
public class PlansController(
    IPlanQueryService planQueryService,
    IStringLocalizer<SubscriptionPlansMessages> localizer) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List the subscription plans")]
    [EndpointDescription("Lists the published plans ordered by device limit, with their coverage and features.")]
    [ProducesResponseType(typeof(IEnumerable<PlanResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPlans(CancellationToken cancellationToken)
    {
        var plans = await planQueryService.Handle(new GetAllPlansQuery(), cancellationToken);
        return Ok(plans.Select(PlanResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{planId:guid}")]
    [EndpointSummary("Get a subscription plan")]
    [EndpointDescription("Gets one subscription plan by its identifier, including plans no longer sold because existing subscriptions refer to them.")]
    [ProducesResponseType(typeof(PlanResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlanById(Guid planId, CancellationToken cancellationToken)
    {
        var result = await planQueryService.Handle(new GetPlanByIdQuery(new PlanId(planId)), cancellationToken);
        return result.IsFailure
            ? SubscriptionPlansActionResultAssembler.ToProblem(this, result.DomainError, localizer)
            : Ok(PlanResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    [HttpGet("{planId:guid}/quote")]
    [EndpointSummary("Quote a subscription plan")]
    [EndpointDescription("Quotes the annual price of a plan for a head count. The total is the annual fee plus the devices cost.")]
    [ProducesResponseType(typeof(PlanQuoteResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> EstimateCost(Guid planId, [FromQuery] int headCount, CancellationToken cancellationToken)
    {
        var result = await planQueryService.Handle(new EstimatePlanCostQuery(new PlanId(planId), headCount), cancellationToken);
        return result.IsFailure
            ? SubscriptionPlansActionResultAssembler.ToProblem(this, result.DomainError, localizer)
            : Ok(PlanQuoteResourceFromValueAssembler.ToResourceFromValue(result.Value!));
    }
}
