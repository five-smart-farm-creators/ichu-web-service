using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;

/// <summary>
///     Maps DateRange values to the REST response contract.
/// </summary>
public static class DateRangeResourceFromValueObjectAssembler
{
    /// <summary>
    ///     Copies analytics values into the response without recalculating domain results.
    /// </summary>
    public static DateRangeResource ToResourceFromValueObject(DateRange value) =>
        new(value.From, value.To);
}
