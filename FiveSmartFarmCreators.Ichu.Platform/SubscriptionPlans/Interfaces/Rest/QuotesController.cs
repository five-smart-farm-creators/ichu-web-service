using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Transform;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest;

[ApiController]
[AllowAnonymous]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Subscription Plans")]
public class QuotesController(
    IPlanQueryService planQueryService,
    IStringLocalizer<SubscriptionPlansMessages> localizer) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Get a subscription plan quote")]
    [EndpointDescription("Returns the annual fee, device unit price, devices cost and total for a plan and head count. Reading a quote does not create a subscription or start a payment.")]
    [ProducesResponseType(typeof(PlanQuoteResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetQuote([FromQuery, BindRequired] Guid planId, [FromQuery, BindRequired] int headCount,
        CancellationToken cancellationToken)
    {
        var result = await planQueryService.Handle(new EstimatePlanCostQuery(new PlanId(planId), headCount), cancellationToken);
        return result.IsFailure
            ? SubscriptionPlansActionResultAssembler.ToProblem(this, result.DomainError, localizer)
            : Ok(PlanQuoteResourceFromValueAssembler.ToResourceFromValue(result.Value!));
    }
}
