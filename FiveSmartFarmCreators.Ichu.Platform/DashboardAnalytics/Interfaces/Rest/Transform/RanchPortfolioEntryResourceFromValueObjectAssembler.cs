using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;

public static class RanchPortfolioEntryResourceFromValueObjectAssembler
{
    public static RanchPortfolioEntryResource ToResourceFromValueObject(RanchPortfolioEntry value) =>
        new(value.RanchId, value.UpdatedAt, value.Source, value.CriticalAlerts, value.WarningAlerts,
            value.InformativeAlerts, value.UnresponsiveTreatmentAnimals, value.OverdueCampaigns);
}
