using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;

/// <summary>
///     Requests a subscription. The expected total lets the server refuse a quote that changed after the customer saw it,
///     and the operation identifier is the client idempotency key.
/// </summary>
public sealed record RequestSubscriptionCommand(
    Guid RanchId,
    PlanId PlanId,
    int HeadCount,
    decimal ExpectedTotal,
    string TermsVersion,
    Guid OperationId,
    string? AcquisitionSource);
