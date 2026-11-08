using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;

public static class TrendSeriesResourceFromValueObjectAssembler
{
    public static TrendSeriesResource ToResourceFromValueObject(TrendSeries value) =>
        new(value.RanchId, value.LotId, DateRangeResourceFromValueObjectAssembler.ToResourceFromValueObject(value.Period),
            value.UpdatedAt, value.Source, value.Metric, value.Unit, value.LotAverage, value.OutlierDeviation,
            value.Points.Select(TrendPointResourceFromValueObjectAssembler.ToResourceFromValueObject).ToList(),
            value.OutlierAnimals, value.MissingReadings, value.CoverageReason, value.UnavailableReason);
}
