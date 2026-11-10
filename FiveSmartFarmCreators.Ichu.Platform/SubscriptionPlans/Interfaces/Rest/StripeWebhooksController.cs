using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Transform;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest;

/// <summary>
///     Receives the payment events of Stripe. The event signature replaces the bearer token, so the endpoint is anonymous.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/v1/payments/stripe/webhooks")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Payments")]
public class StripeWebhooksController(
    ISubscriptionCommandService subscriptionCommandService,
    IStringLocalizer<SubscriptionPlansMessages> localizer,
    ILogger<StripeWebhooksController> logger) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Receive a Stripe payment event")]
    [EndpointDescription("Verifies the Stripe-Signature header and activates the subscription of a completed payment. Events that change no subscription, and payments that cannot be applied, are acknowledged with 200 so that Stripe stops retrying them.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ReceivePaymentEvent(CancellationToken cancellationToken)
    {
        // Stripe signs the exact bytes it sent, so the body is read as text rather than bound to a model.
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var result = await subscriptionCommandService.Handle(
            new ReceivePaymentEventCommand(payload, Request.Headers["Stripe-Signature"].ToString()), cancellationToken);
        if (result.IsSuccess || result.DomainError == SubscriptionPlansErrors.PaymentEventIgnored)
        {
            return Ok();
        }

        // A forged event is refused, and a missing configuration is unavailable until an operator fixes it.
        if (result.DomainError == SubscriptionPlansErrors.PaymentSignatureInvalid
            || result.DomainError == SubscriptionPlansErrors.PaymentProviderNotConfigured)
        {
            return SubscriptionPlansActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        // Stripe stops retrying a payment that the domain refused, such as an unknown subscription, so the reason is logged.
        logger.LogWarning("A Stripe payment event was not applied: {ErrorCode}.", result.DomainError.Code);
        return Ok();
    }
}
