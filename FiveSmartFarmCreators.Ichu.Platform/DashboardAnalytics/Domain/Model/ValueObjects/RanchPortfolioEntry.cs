namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

/// <summary>
///     Summarizes ranch attention priorities with source freshness information.
/// </summary>
public sealed record RanchPortfolioEntry(Guid RanchId, DateTimeOffset UpdatedAt, string Source,
    int CriticalAlerts, int WarningAlerts, int InformativeAlerts, int? UnresponsiveTreatmentAnimals, int? OverdueCampaigns);
