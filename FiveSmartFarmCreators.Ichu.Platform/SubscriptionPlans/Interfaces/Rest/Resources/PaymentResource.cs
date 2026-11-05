using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Entities;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;

public sealed record PaymentResource(
    Guid PaymentId,
    string Purpose,
    MoneyResource Amount,
    string Status,
    DateTimeOffset RequestedAt,
    DateTimeOffset? ProcessedAt,
    string? ExternalReference,
    string? RejectionReason)
{
    public static PaymentResource From(Payment payment) =>
        new(
            payment.Id,
            payment.Purpose.ToString(),
            MoneyResource.From(payment.Amount),
            payment.Status.ToString(),
            payment.RequestedAt,
            payment.ProcessedAt,
            payment.ExternalReference,
            payment.RejectionReason);
}
