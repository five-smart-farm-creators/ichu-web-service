using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest;

[ApiController]
[Route("api/v1/payment-confirmations")]
[TypeFilter(typeof(SubscriptionPlansExceptionFilter))]
public sealed class PaymentConfirmationsController(PaymentConfirmationService confirmations) : ControllerBase
{
    private const int MaximumPayloadBytes = 64 * 1024;

    /// <summary>
    /// Receives a payment-provider notification. Its authenticity is verified by the provider adapter; an
    /// unverified notification returns 401 and changes nothing. A repeated result returns 200.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [RequestSizeLimit(MaximumPayloadBytes)]
    public async Task<IActionResult> Receive(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString(), StringComparer.OrdinalIgnoreCase);

        var outcome = await confirmations.ProcessAsync(new ProcessPaymentNotificationCommand(payload, headers), cancellationToken);
        return outcome == ChangeOutcome.Conflict
            ? Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "The payment was already processed with a different result or amount." })
            : Ok(new { outcome = outcome.ToString() });
    }
}
