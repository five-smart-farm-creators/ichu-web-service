using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms IAM boundary representations without invoking business operations.</summary>
public static class CompleteProfessionalProfileCommandFromResourceAssembler
{
    /// <summary>Maps the supplied representation to the target contract.</summary>
    public static CompleteProfessionalProfileCommand ToCommandFromResource(CompleteProfileResource resource) =>
        new(resource.Specialty, resource.LicenseNumber, resource.YearsOfExperience);
}
