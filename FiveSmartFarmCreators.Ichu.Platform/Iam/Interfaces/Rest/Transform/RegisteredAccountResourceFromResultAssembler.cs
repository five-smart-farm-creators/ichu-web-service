using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms IAM boundary representations without invoking business operations.</summary>
public static class RegisteredAccountResourceFromResultAssembler
{
    /// <summary>Maps the supplied representation to the target contract.</summary>
    public static RegisteredAccountResource ToResourceFromResult(RegisteredAccount result) =>
        new(AccountResourceFromEntityAssembler.ToResourceFromEntity(result.Account), result.RanchId);
}
