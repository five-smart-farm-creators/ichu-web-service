using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;

/// <summary>
///     Maps RanchPortfolioEntry values to the REST response contract.
/// </summary>
public static class RanchPortfolioEntryResourceFromValueObjectAssembler
{
    /// <summary>
    ///     Copies analytics values into the response without recalculating domain results.
    /// </summary>
    public static RanchPortfolioEntryResource ToResourceFromValueObject(RanchPortfolioEntry value) =>
        new(value.RanchId, value.UpdatedAt, value.Source, value.CriticalAlerts, value.WarningAlerts,
            value.InformativeAlerts, value.UnresponsiveTreatmentAnimals, value.OverdueCampaigns);
}
