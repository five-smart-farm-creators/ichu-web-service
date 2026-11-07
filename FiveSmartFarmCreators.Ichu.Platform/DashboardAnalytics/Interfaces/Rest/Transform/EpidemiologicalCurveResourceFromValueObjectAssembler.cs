using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;

/// <summary>
///     Maps EpidemiologicalCurve values to the REST response contract.
/// </summary>
public static class EpidemiologicalCurveResourceFromValueObjectAssembler
{
    /// <summary>
    ///     Copies analytics values into the response without recalculating domain results.
    /// </summary>
    public static EpidemiologicalCurveResource ToResourceFromValueObject(EpidemiologicalCurve value) =>
        new(value.RanchId, DateRangeResourceFromValueObjectAssembler.ToResourceFromValueObject(value.Period),
            value.UpdatedAt, value.Source, value.Condition, value.NewCasesByDay, value.DaysWithoutNewCases,
            value.SupportsRestrictionLifting, value.UnavailableReason);
}
