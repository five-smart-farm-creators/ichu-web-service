namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Queries;

/// <summary>
///     Reads the pending or active subscription of a ranch.
/// </summary>
public sealed record GetSubscriptionByRanchQuery(Guid RanchId);
