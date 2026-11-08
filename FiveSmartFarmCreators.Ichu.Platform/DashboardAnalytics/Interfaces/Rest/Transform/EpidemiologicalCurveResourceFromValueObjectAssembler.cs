using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;

public static class EpidemiologicalCurveResourceFromValueObjectAssembler
{
    public static EpidemiologicalCurveResource ToResourceFromValueObject(EpidemiologicalCurve value) =>
        new(value.RanchId, DateRangeResourceFromValueObjectAssembler.ToResourceFromValueObject(value.Period),
            value.UpdatedAt, value.Source, value.Condition, value.NewCasesByDay, value.DaysWithoutNewCases,
            value.SupportsRestrictionLifting, value.UnavailableReason);
}
