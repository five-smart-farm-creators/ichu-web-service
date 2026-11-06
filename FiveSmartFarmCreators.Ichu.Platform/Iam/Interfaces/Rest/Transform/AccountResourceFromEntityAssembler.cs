using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms IAM boundary representations without invoking business operations.</summary>
public static class AccountResourceFromEntityAssembler
{
    /// <summary>Maps the supplied representation to the target contract.</summary>
    public static AccountResource ToResourceFromEntity(UserAccount account) =>
        new(account.Id, account.Email.Value, account.Role.ToString(), account.Status.ToString(),
            ProfessionalProfileResourceFromEntityAssembler.ToResourceFromEntity(account.Profile));
}
