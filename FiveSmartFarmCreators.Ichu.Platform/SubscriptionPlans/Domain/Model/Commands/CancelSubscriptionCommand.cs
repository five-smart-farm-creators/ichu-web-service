using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;

public sealed record CancelSubscriptionCommand(Guid RanchId, SubscriptionId SubscriptionId, string Reason);
