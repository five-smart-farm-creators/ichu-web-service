namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application;

/// <summary>Settings bound from the "SubscriptionPlans" configuration section.</summary>
public sealed class SubscriptionPlansOptions
{
    public const string SectionName = "SubscriptionPlans";

    /// <summary>The terms and conditions version a requester must accept. Requests are unavailable until it is configured.</summary>
    public string? CurrentTermsVersion { get; set; }
}
