using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;

public static class IndicatorValueResourceFromValueObjectAssembler
{
    public static IndicatorValueResource ToResourceFromValueObject(IndicatorValue value) =>
        new(value.Value, value.Unit, value.Formula, value.Denominator, value.UnavailableReason);
}
