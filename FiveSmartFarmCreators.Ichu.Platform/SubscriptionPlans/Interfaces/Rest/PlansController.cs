using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest;

[ApiController]
[Route("api/v1/plans")]
[AllowAnonymous]
[Produces("application/json")]
[TypeFilter(typeof(SubscriptionPlansExceptionFilter))]
public sealed class PlansController(PlanCatalogService catalog) : ControllerBase
{
    /// <summary>Lists the plans currently offered at a valid price.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PlanResource>>> GetPlans(CancellationToken cancellationToken) =>
        Ok((await catalog.GetOfferedPlansAsync(cancellationToken)).Select(PlanResource.From).ToList());

    [HttpGet("{planId:guid}")]
    public async Task<ActionResult<PlanResource>> GetPlan(Guid planId, CancellationToken cancellationToken) =>
        Ok(PlanResource.From(await catalog.GetPlanAsync(planId, cancellationToken)));

    /// <summary>
    /// Estimates the cost for a head count. A count outside the plan coverage returns 400 with the restriction and,
    /// only when another plan covers it, a suggestedPlanId extension.
    /// </summary>
    [HttpGet("{planId:guid}/quote")]
    [ProducesResponseType<PlanQuoteResource>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlanQuoteResource>> Quote(Guid planId, [FromQuery] int headCount, CancellationToken cancellationToken)
    {
        var result = await catalog.QuoteAsync(planId, headCount, cancellationToken);
        if (result.Quote is not null)
        {
            return Ok(PlanQuoteResource.From(result.Quote));
        }

        var problem = new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = result.Restriction };
        problem.Extensions["suggestedPlanId"] = result.SuggestedPlanId;
        return BadRequest(problem);
    }
}
