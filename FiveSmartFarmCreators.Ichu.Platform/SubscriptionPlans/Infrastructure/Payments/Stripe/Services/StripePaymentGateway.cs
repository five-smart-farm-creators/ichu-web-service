using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Payments.Stripe.Configuration;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using Subscription = FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates.Subscription;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Payments.Stripe.Services;

/// <summary>
///     Adapts Stripe Checkout and webhook verification to the payment port. Stripe exceptions end here, so the rest of the
///     context only sees Subscription Plans error codes.
/// </summary>
public sealed class StripePaymentGateway(
    IOptions<StripeOptions> options,
    TimeProvider clock,
    ILogger<StripePaymentGateway> logger) : IPaymentGateway
{
    private const string SolesCurrency = "PEN";
    private const string StripeSolesCurrency = "pen";
    private const string PaidPaymentStatus = "paid";
    private const string CheckoutCompletedEvent = "checkout.session.completed";
    private const string AsyncPaymentSucceededEvent = "checkout.session.async_payment_succeeded";
    private const string SessionIdPlaceholder = "session_id={CHECKOUT_SESSION_ID}";
    // Stripe rejects sessions that expire in less than 30 minutes; one more minute absorbs the request latency.
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromMinutes(31);

    /// <inheritdoc />
    public async Task<Result<CheckoutSession>> CreateCheckoutSessionAsync(Subscription subscription, string planName, CancellationToken cancellationToken)
    {
        var settings = CheckoutSettings();
        if (settings.IsFailure)
        {
            return Result<CheckoutSession>.Failure(settings.DomainError);
        }

        // Plans are priced in soles, so a total in another currency would be charged under the wrong currency code.
        if (subscription.Total.Currency != SolesCurrency)
        {
            throw new InvalidOperationException("Checkout sessions can only charge subscriptions priced in PEN.");
        }

        try
        {
            var checkout = settings.Value!;
            var sessions = new SessionService(new StripeClient(checkout.SecretKey));
            var session = await sessions.CreateAsync(CheckoutParameters(subscription, planName, checkout),
                cancellationToken: cancellationToken);
            return Result<CheckoutSession>.Success(new CheckoutSession(
                session.Id,
                new Uri(session.Url),
                new DateTimeOffset(DateTime.SpecifyKind(session.ExpiresAt, DateTimeKind.Utc))));
        }
        catch (Exception error) when (IsProviderFailure(error, cancellationToken))
        {
            logger.LogWarning(error, "The payment provider did not create a checkout session for subscription {SubscriptionId}.",
                subscription.Id.Value);
            return Result<CheckoutSession>.Failure(SubscriptionPlansErrors.PaymentProviderUnavailable);
        }
    }

    /// <inheritdoc />
    public Result<PaymentConfirmation> ReadConfirmation(string payload, string signatureHeader)
    {
        var webhookSecret = options.Value.WebhookSecret;
        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            return Result<PaymentConfirmation>.Failure(SubscriptionPlansErrors.PaymentProviderNotConfigured);
        }

        Event stripeEvent;
        try
        {
            // Endpoints deliver the API version they were created with, which may differ from this SDK; the signature still proves the sender.
            stripeEvent = EventUtility.ConstructEvent(payload, signatureHeader, webhookSecret, throwOnApiVersionMismatch: false);
        }
        catch (StripeException)
        {
            return Result<PaymentConfirmation>.Failure(SubscriptionPlansErrors.PaymentSignatureInvalid);
        }

        if (stripeEvent.Type is not (CheckoutCompletedEvent or AsyncPaymentSucceededEvent)
            || stripeEvent.Data.Object is not Session session
            || !Guid.TryParse(session.ClientReferenceId, out var subscriptionId))
        {
            return Result<PaymentConfirmation>.Failure(SubscriptionPlansErrors.PaymentEventIgnored);
        }

        return Result<PaymentConfirmation>.Success(new PaymentConfirmation(
            stripeEvent.Id,
            session.Id,
            new SubscriptionId(subscriptionId),
            session.AmountTotal ?? 0,
            session.Currency.ToUpperInvariant(),
            session.PaymentStatus == PaidPaymentStatus));
    }

    private Result<StripeOptions> CheckoutSettings()
    {
        var settings = options.Value;
        return string.IsNullOrWhiteSpace(settings.SecretKey)
            || string.IsNullOrWhiteSpace(settings.SuccessUrl)
            || string.IsNullOrWhiteSpace(settings.CancelUrl)
            ? Result<StripeOptions>.Failure(SubscriptionPlansErrors.PaymentProviderNotConfigured)
            : Result<StripeOptions>.Success(settings);
    }

    private SessionCreateOptions CheckoutParameters(Subscription subscription, string planName, StripeOptions settings)
    {
        var subscriptionId = subscription.Id.Value.ToString();
        return new SessionCreateOptions
        {
            Mode = "payment",
            AllowedPaymentMethodTypes = ["card"],
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = StripeSolesCurrency,
                        UnitAmount = subscription.Total.ToMinorUnits(),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"ICHU {planName} plan — annual subscription"
                        }
                    }
                }
            ],
            ClientReferenceId = subscriptionId,
            Metadata = new Dictionary<string, string>
            {
                ["subscription_id"] = subscriptionId,
                ["ranch_id"] = subscription.RanchId.ToString()
            },
            SuccessUrl = AppendSessionIdPlaceholder(settings.SuccessUrl),
            CancelUrl = settings.CancelUrl,
            ExpiresAt = clock.GetUtcNow().Add(SessionLifetime).UtcDateTime
        };
    }

    // Stripe replaces the placeholder with the session identifier when it sends the customer back to the application.
    private static string AppendSessionIdPlaceholder(string successUrl) =>
        successUrl + (successUrl.Contains('?') ? '&' : '?') + SessionIdPlaceholder;

    // A client timeout cancels the request although the caller did not ask for it, so it counts as a provider failure.
    private static bool IsProviderFailure(Exception error, CancellationToken cancellationToken) =>
        error is StripeException or HttpRequestException
        || (error is OperationCanceledException && !cancellationToken.IsCancellationRequested);
}
