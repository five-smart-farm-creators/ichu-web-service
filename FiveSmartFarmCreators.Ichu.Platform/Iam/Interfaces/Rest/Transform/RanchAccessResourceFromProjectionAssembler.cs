using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms IAM boundary representations without invoking business operations.</summary>
public static class RanchAccessResourceFromProjectionAssembler
{
    /// <summary>Maps the supplied representation to the target contract.</summary>
    public static RanchAccessResource ToResourceFromProjection(RanchAccess access) =>
        new(access.RanchId, access.Scope.ToString(), access.ExpiresAt);
}
