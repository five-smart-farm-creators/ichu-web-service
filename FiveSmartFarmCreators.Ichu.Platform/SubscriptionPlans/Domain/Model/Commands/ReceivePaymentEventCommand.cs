namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;

/// <summary>
///     Carries a provider event exactly as it was received, so that its signature is verified before it changes a subscription.
/// </summary>
public sealed record ReceivePaymentEventCommand(string Payload, string SignatureHeader);
