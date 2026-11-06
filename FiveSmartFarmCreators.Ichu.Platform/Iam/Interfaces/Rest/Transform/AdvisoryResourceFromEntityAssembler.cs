using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms IAM boundary representations without invoking business operations.</summary>
public static class AdvisoryResourceFromEntityAssembler
{
    /// <summary>Maps the supplied representation to the target contract.</summary>
    public static AdvisoryResource ToResourceFromEntity(HerdAdvisory advisory) =>
        new(advisory.Id, advisory.VeterinarianId, advisory.RanchId, advisory.Reason, advisory.Status.ToString(),
            advisory.Scope.ToString(), advisory.RequestedAt, advisory.GrantedAt, advisory.ExpiresAt, advisory.EndedAt, null);
}
