namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Acl;

public sealed record SubscriptionEntitlements(bool IsActive, int MaximumDevices, IReadOnlyList<string> Features);
