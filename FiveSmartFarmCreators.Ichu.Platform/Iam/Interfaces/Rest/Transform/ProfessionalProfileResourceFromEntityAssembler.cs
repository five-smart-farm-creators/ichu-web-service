using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms IAM boundary representations without invoking business operations.</summary>
public static class ProfessionalProfileResourceFromEntityAssembler
{
    /// <summary>Maps the supplied representation to the target contract.</summary>
    public static ProfessionalProfileResource? ToResourceFromEntity(ProfessionalProfile? profile) => profile is null ? null :
        new(profile.Specialty, profile.LicenseNumber, profile.YearsOfExperience);
}
