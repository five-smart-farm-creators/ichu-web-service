namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;

/// <summary>
///     Body of a subscription request. The expected total is the quote the customer accepted, and the operation
///     identifier makes a retry safe.
/// </summary>
public sealed record RequestSubscriptionResource(
    Guid PlanId,
    int HeadCount,
    decimal ExpectedTotal,
    string TermsVersion,
    Guid OperationId,
    string? AcquisitionSource = null);
