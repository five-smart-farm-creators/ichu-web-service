using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest;

[ApiController]
[Route("api/v1/subscriptions")]
[Authorize]
[Produces("application/json")]
[TypeFilter(typeof(SubscriptionPlansExceptionFilter))]
public sealed class SubscriptionsController(SubscriptionService subscriptions) : ControllerBase
{
    /// <summary>
    /// Registers a Pending subscription request with its pending activation payment. Repeating the same operation
    /// returns 200 with the existing request; the plan is enabled only after a verified payment.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SubscriptionResource>> RequestSubscription(RequestSubscriptionResource resource, CancellationToken cancellationToken)
    {
        var result = await subscriptions.RequestAsync(resource.ToCommand(), cancellationToken);
        var body = SubscriptionResource.From(result.Details);
        return result.Created ? Created($"/api/v1/subscriptions/{body.SubscriptionId}", body) : Ok(body);
    }

    /// <summary>Returns the pending or active subscription of the ranch, or its latest one when none is open.</summary>
    [HttpGet]
    public async Task<ActionResult<SubscriptionResource>> GetByRanch([FromQuery] Guid ranchId, CancellationToken cancellationToken) =>
        Ok(SubscriptionResource.From(await subscriptions.GetCurrentAsync(ranchId, cancellationToken)));

    [HttpGet("{subscriptionId:guid}")]
    public async Task<ActionResult<SubscriptionResource>> Get(Guid subscriptionId, CancellationToken cancellationToken) =>
        Ok(SubscriptionResource.From(await subscriptions.GetAsync(subscriptionId, cancellationToken)));

    /// <summary>Cancels the subscription immediately and keeps its payment history.</summary>
    [HttpPost("{subscriptionId:guid}/cancellations")]
    public async Task<ActionResult<SubscriptionResource>> Cancel(Guid subscriptionId, CancelSubscriptionResource resource, CancellationToken cancellationToken) =>
        Ok(SubscriptionResource.From(await subscriptions.CancelAsync(subscriptionId, resource.Reason ?? string.Empty, cancellationToken)));

    [HttpGet("{subscriptionId:guid}/payments")]
    public async Task<ActionResult<IReadOnlyList<PaymentResource>>> GetPayments(Guid subscriptionId, CancellationToken cancellationToken) =>
        Ok((await subscriptions.GetPaymentsAsync(subscriptionId, cancellationToken)).Select(PaymentResource.From).ToList());

    /// <summary>Requests the next activation or renewal payment. The provider must confirm it before it takes effect.</summary>
    [HttpPost("{subscriptionId:guid}/payments")]
    public async Task<ActionResult<PaymentResource>> RequestPayment(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var payment = await subscriptions.RequestPaymentAsync(subscriptionId, cancellationToken);
        return Created($"/api/v1/subscriptions/{subscriptionId}/payments", PaymentResource.From(payment));
    }
}
