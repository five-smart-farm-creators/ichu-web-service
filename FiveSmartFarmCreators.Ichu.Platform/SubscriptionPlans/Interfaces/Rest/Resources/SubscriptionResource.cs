namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;

/// <summary>
///     Subscription of a ranch with its status, validity, the device limit and features of its plan, and the agreed amounts.
/// </summary>
public sealed record SubscriptionResource(
    Guid Id,
    Guid RanchId,
    Guid PlanId,
    string Status,
    int HeadCount,
    MoneyResource AnnualFee,
    MoneyResource DevicesCost,
    MoneyResource Total,
    string TermsVersion,
    DateTimeOffset RequestedAt,
    DateTimeOffset? ActivatedAt,
    DateTimeOffset? ValidUntil,
    DateTimeOffset? CancelledAt,
    string? CancellationReason,
    int MaximumDevices,
    IReadOnlyList<string> Features);
