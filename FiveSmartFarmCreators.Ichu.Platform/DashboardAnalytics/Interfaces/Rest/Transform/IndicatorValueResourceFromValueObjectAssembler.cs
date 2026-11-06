using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;

/// <summary>
///     Maps IndicatorValue values to the REST response contract.
/// </summary>
public static class IndicatorValueResourceFromValueObjectAssembler
{
    /// <summary>
    ///     Copies analytics values into the response without recalculating domain results.
    /// </summary>
    public static IndicatorValueResource ToResourceFromValueObject(IndicatorValue value) =>
        new(value.Value, value.Unit, value.Formula, value.Denominator, value.UnavailableReason);
}
