namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Ports;

/// <summary>A payment result whose authenticity the provider adapter has verified.</summary>
public sealed record VerifiedPaymentResult(
    Guid PaymentId,
    bool Accepted,
    string ExternalReference,
    decimal Amount,
    string Currency,
    string? RejectionReason,
    DateTimeOffset ProcessedAt);
