using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

public static class ProfessionalProfileResourceFromEntityAssembler
{
    public static ProfessionalProfileResource? ToResourceFromEntity(ProfessionalProfile? profile) => profile is null ? null :
        new(profile.Specialty, profile.LicenseNumber, profile.YearsOfExperience);
}
