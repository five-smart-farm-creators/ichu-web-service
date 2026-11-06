using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

public static class RanchAccessResourceFromProjectionAssembler
{
    public static RanchAccessResource ToResourceFromProjection(RanchAccess access) =>
        new(access.RanchId, access.Scope.ToString(), access.ExpiresAt);
}
