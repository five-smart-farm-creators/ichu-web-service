using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

public static class RanchSummaryResourceFromEntityAssembler
{
    public static RanchSummaryResource ToResourceFromEntity(RanchRegistration ranch) =>
        new(ranch.Id, ranch.Name, ranch.Region, ranch.District);
}
