using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

public static class RanchAccessResourceFromProjectionAssembler
{
    public static RanchAccessResource ToResourceFromProjection(AccountRanch ranch) =>
        new(ranch.Access.RanchId, ranch.Access.Scope.ToString(), ranch.Access.ExpiresAt, ranch.Name, ranch.Region,
            ranch.District, ranch.AccessCode?.Value);
}
