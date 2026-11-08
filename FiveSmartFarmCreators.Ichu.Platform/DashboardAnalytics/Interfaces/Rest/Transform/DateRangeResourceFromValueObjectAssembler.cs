using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;

public static class DateRangeResourceFromValueObjectAssembler
{
    public static DateRangeResource ToResourceFromValueObject(DateRange value) =>
        new(value.From, value.To);
}
