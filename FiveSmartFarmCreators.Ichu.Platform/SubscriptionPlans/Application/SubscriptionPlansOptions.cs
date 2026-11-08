namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application;

public class SubscriptionPlansOptions
{
    public const string SectionName = "SubscriptionPlans";

    /// <summary>
    ///     Gets or sets the terms version a subscription request must accept. Changing it forces clients to read the new terms.
    /// </summary>
    public string CurrentTermsVersion { get; set; } = string.Empty;
}
