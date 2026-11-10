using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

public static class AdvisoryResourceFromEntityAssembler
{
    public static AdvisoryResource ToResourceFromEntity(HerdAdvisory advisory) =>
        new(advisory.Id.Value, advisory.VeterinarianId.Value, advisory.RanchId, advisory.Reason, advisory.Status.ToString(),
            advisory.Scope.ToString(), advisory.RequestedAt, advisory.GrantedAt, advisory.ExpiresAt, advisory.EndedAt, null);
}
