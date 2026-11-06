using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

public static class AdvisoryResourceFromDetailsAssembler
{
    public static AdvisoryResource ToResourceFromDetails(HerdAdvisoryDetails advisory) =>
        new(advisory.Id, advisory.VeterinarianId, advisory.RanchId, advisory.Reason, advisory.Status.ToString(),
            advisory.Scope.ToString(), advisory.RequestedAt, advisory.GrantedAt, advisory.ExpiresAt, advisory.EndedAt,
            ProfessionalProfileResourceFromEntityAssembler.ToResourceFromEntity(advisory.VeterinarianProfile));
}
