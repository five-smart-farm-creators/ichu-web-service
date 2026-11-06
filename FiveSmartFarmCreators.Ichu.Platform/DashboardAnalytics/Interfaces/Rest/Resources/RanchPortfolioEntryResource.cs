namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

/// <summary>
///     Represents the REST response data for ranch portfolio entry.
/// </summary>
public sealed record RanchPortfolioEntryResource(Guid RanchId, DateTimeOffset UpdatedAt, string Source,
    int CriticalAlerts, int WarningAlerts, int InformativeAlerts, int? UnresponsiveTreatmentAnimals, int? OverdueCampaigns);
