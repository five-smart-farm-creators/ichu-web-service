namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application;

public class DashboardAnalyticsOptions
{
    public const string SectionName = "DashboardAnalytics";

    /// <summary>
    ///     Incubation days per health condition. The shipped values are placeholders that the team must validate, and a condition without a value has no progression assessment.
    /// </summary>
    public Dictionary<string, int> ConditionIncubationDays { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
