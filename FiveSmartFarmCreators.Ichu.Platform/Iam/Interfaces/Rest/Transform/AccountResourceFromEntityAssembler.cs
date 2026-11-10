using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

public static class AccountResourceFromEntityAssembler
{
    public static AccountResource ToResourceFromEntity(UserAccount account) =>
        new(account.Id.Value, account.Email.Value, account.Role.ToString(), account.Status.ToString(),
            ProfessionalProfileResourceFromEntityAssembler.ToResourceFromEntity(account.Profile));
}
