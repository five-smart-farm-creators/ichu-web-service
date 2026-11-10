using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;
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
///     Requests, reads and cancels the subscriptions of ranches. The application layer checks the ranch permission on
///     every action.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/ranches/{ranchId:guid}/subscriptions")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Subscriptions")]
public class SubscriptionsController(
    ISubscriptionCommandService subscriptionCommandService,
    ISubscriptionQueryService subscriptionQueryService,
    IPlanQueryService planQueryService,
    IStringLocalizer<SubscriptionPlansMessages> localizer) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Request a subscription")]
    [EndpointDescription("Requests a pending subscription at the quoted price. A retry with the same operation identifier returns 200 with the stored subscription.")]
    [ProducesResponseType(typeof(SubscriptionResource), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(SubscriptionResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RequestSubscription(Guid ranchId, RequestSubscriptionResource resource, CancellationToken cancellationToken)
    {
        var command = RequestSubscriptionCommandFromResourceAssembler.ToCommandFromResource(ranchId, resource);
        var result = await subscriptionCommandService.Handle(command, cancellationToken);
        if (result.IsFailure)
        {
            return SubscriptionPlansActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        var outcome = result.Value!;
        var subscription = await ToResourceAsync(outcome.Subscription, cancellationToken);
        return outcome.Created
            ? CreatedAtAction(nameof(GetSubscriptionByRanch), new { ranchId = subscription.RanchId }, subscription)
            : Ok(subscription);
    }

    [HttpGet]
    [EndpointSummary("Get the subscription of a ranch")]
    [EndpointDescription("Reads the pending or active subscription of a ranch, with its validity, device limit, features and amounts.")]
    [ProducesResponseType(typeof(SubscriptionResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubscriptionByRanch(Guid ranchId, CancellationToken cancellationToken)
    {
        var result = await subscriptionQueryService.Handle(new GetSubscriptionByRanchQuery(ranchId), cancellationToken);
        if (result.IsFailure)
        {
            return SubscriptionPlansActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(await ToResourceAsync(result.Value!, cancellationToken));
    }

    [HttpPost("{subscriptionId:guid}/cancellations")]
    [EndpointSummary("Cancel a subscription")]
    [EndpointDescription("Cancels a pending or active subscription with a reason of 1 to 200 characters.")]
    [ProducesResponseType(typeof(SubscriptionResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelSubscription(Guid ranchId, Guid subscriptionId, CancelSubscriptionResource resource, CancellationToken cancellationToken)
    {
        var command = new CancelSubscriptionCommand(ranchId, new SubscriptionId(subscriptionId), resource.Reason);
        var result = await subscriptionCommandService.Handle(command, cancellationToken);
        if (result.IsFailure)
        {
            return SubscriptionPlansActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(await ToResourceAsync(result.Value!, cancellationToken));
    }

    private async Task<SubscriptionResource> ToResourceAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        // Subscriptions refer only to seeded plans, so a missing plan means corrupted data rather than a business error.
        var plan = await planQueryService.Handle(new GetPlanByIdQuery(subscription.PlanId), cancellationToken);
        return SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription,
            plan.Value ?? throw new InvalidOperationException("A subscription refers to a plan that does not exist."));
    }
}
