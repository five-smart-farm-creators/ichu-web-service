using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;

/// <summary>
/// A ranch subscription. Status is Pending, Active, Cancelled or Expired; limits and features are enabled only
/// while IsEntitled is true, which requires an Active status within the paid period.
/// </summary>
public sealed record SubscriptionResource(
    Guid SubscriptionId,
    Guid RanchId,
    Guid PlanId,
    string PlanCode,
    int HeadCount,
    string Status,
    bool IsEntitled,
    string BillingPeriod,
    MoneyResource RecurringAmount,
    DateTimeOffset RequestedAt,
    DateTimeOffset? ActivatedAt,
    DateTimeOffset? PaidUntil,
    DateTimeOffset? CancelledAt,
    string? CancellationReason,
    int MaximumDevices,
    IReadOnlyList<string> Features,
    string TermsVersion,
    PaymentResource? PendingPayment)
{
    public static SubscriptionResource From(SubscriptionDetails details)
    {
        var subscription = details.Subscription;
        var pending = subscription.Payments.FirstOrDefault(p => p.Status == Domain.Model.ValueObjects.PaymentStatus.Pending);
        return new SubscriptionResource(
            subscription.Id,
            subscription.RanchId,
            subscription.PlanId,
            details.Plan.Code,
            subscription.HeadCount,
            subscription.StatusAt(details.EvaluatedAt).ToString(),
            subscription.IsEntitledAt(details.EvaluatedAt),
            subscription.BillingPeriod.ToString(),
            MoneyResource.From(subscription.RecurringAmount),
            subscription.RequestedAt,
            subscription.ActivatedAt,
            subscription.PaidUntil,
            subscription.CancelledAt,
            subscription.CancellationReason,
            details.Plan.MaximumDevices,
            details.Plan.Features.Select(f => f.Code.ToString()).Order(StringComparer.Ordinal).ToList(),
            subscription.TermsVersion,
            pending is null ? null : PaymentResource.From(pending));
    }
}
