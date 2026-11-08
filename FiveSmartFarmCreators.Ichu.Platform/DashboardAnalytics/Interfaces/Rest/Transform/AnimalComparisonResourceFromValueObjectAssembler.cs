using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;

public static class AnimalComparisonResourceFromValueObjectAssembler
{
    public static AnimalComparisonResource ToResourceFromValueObject(AnimalComparison value) =>
        new(value.AnimalId, TrendSeriesResourceFromValueObjectAssembler.ToResourceFromValueObject(value.Reference),
            value.AnimalAverage, value.Deviation, value.ReferenceAnimalCount, value.UnavailableReason);
}
