using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;

public static class TrendPointResourceFromValueObjectAssembler
{
    public static TrendPointResource ToResourceFromValueObject(TrendPoint value) =>
        new(value.AnimalId, value.RecordedAt, value.Value);
}
