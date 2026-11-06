using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

public static class RegisteredAccountResourceFromResultAssembler
{
    public static RegisteredAccountResource ToResourceFromResult(RegisteredAccount result) =>
        new(AccountResourceFromEntityAssembler.ToResourceFromEntity(result.Account), result.RanchId);
}
