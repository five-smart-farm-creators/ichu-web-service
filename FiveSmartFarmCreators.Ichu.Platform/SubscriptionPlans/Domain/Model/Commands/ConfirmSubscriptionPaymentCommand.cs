using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;

/// <summary>
///     Applies a payment that the provider confirmed for a subscription. It is never exposed over HTTP without the
///     signature check of the webhook.
/// </summary>
public sealed record ConfirmSubscriptionPaymentCommand(PaymentConfirmation Confirmation);
