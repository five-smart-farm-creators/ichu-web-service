using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Acl;

/// <summary>
/// What a ranch may use now. Without an active, paid subscription the entitlement is inactive, with no devices
/// and no features, so consumers fail closed.
/// </summary>
public sealed record RanchEntitlement(
    Guid RanchId,
    bool IsActive,
    Guid? SubscriptionId,
    Guid? PlanId,
    string? PlanCode,
    int MaximumDevices,
    IReadOnlySet<FeatureCode> Features,
    DateTimeOffset? PaidUntil)
{
    public static RanchEntitlement Inactive(Guid ranchId) => new(ranchId, false, null, null, null, 0, new HashSet<FeatureCode>(), null);
}
