namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;

/// <summary>A raw payment-provider notification; it has no effect until the provider adapter verifies it.</summary>
public sealed record ProcessPaymentNotificationCommand(string Payload, IReadOnlyDictionary<string, string> Headers);
