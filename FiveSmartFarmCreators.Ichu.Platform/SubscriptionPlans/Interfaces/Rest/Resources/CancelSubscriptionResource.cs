namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;

/// <summary>
///     Body of a cancellation request, whose reason must hold between 1 and 200 characters.
/// </summary>
public sealed record CancelSubscriptionResource(string Reason);
