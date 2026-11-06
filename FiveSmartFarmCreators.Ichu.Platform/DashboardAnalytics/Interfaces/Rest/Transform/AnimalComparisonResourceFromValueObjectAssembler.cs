using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;

/// <summary>
///     Maps AnimalComparison values to the REST response contract.
/// </summary>
public static class AnimalComparisonResourceFromValueObjectAssembler
{
    /// <summary>
    ///     Copies analytics values into the response without recalculating domain results.
    /// </summary>
    public static AnimalComparisonResource ToResourceFromValueObject(AnimalComparison value) =>
        new(value.AnimalId, TrendSeriesResourceFromValueObjectAssembler.ToResourceFromValueObject(value.Reference),
            value.AnimalAverage, value.Deviation, value.ReferenceAnimalCount, value.UnavailableReason);
}
