namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

public sealed record RanchPortfolioEntryResource(Guid RanchId, DateTimeOffset UpdatedAt, string Source,
    int CriticalAlerts, int WarningAlerts, int InformativeAlerts, int? UnresponsiveTreatmentAnimals, int? OverdueCampaigns);
