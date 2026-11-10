namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Payments.Stripe.Configuration;

/// <summary>
///     Stripe settings. The secret values come from user environment variables or secrets, never from source control.
/// </summary>
public sealed class StripeOptions
{
    public const string SectionName = "Stripe";

    public string SecretKey { get; set; } = string.Empty;

    public string WebhookSecret { get; set; } = string.Empty;

    public string SuccessUrl { get; set; } = string.Empty;

    public string CancelUrl { get; set; } = string.Empty;
}
